#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationManager\ConfigurationManager.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ConfigurationManager : IConfigurationManager
    {
        internal readonly ConfigurationLayerCollection Layers;
        internal readonly YodiiEngine Engine;

        FinalConfiguration _finalConfiguration;
        IDiscoveredInfo _discoveredInfo;

        ConfigurationChangingEventArgs _currentEventArgs;

        public event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        IConfigurationLayerCollection IConfigurationManager.Layers
        {
            get { return Layers; }
        }

        internal ConfigurationManager( YodiiEngine engine )
        {
            Engine = engine;
            Layers = new ConfigurationLayerCollection( this );
            _finalConfiguration = new FinalConfiguration();
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
        }


        /// <summary>
        /// Read-only collection container of read-only configuration items.
        /// </summary>
        /// <remarks>
        /// This final configuration is automatically maintained.
        /// Any change to the configuration can be canceled thanks to <see cref="IConfigurationManager.ConfigurationChanging"/>.
        /// </remarks>
        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
            private set
            {
                _finalConfiguration = value;
                RaisePropertyChanged();
            }
        }

        /// <summary>
        /// Gets the current information about available plugins and services.
        /// </summary>
        public IDiscoveredInfo DiscoveredInfo 
        {
            get { return _discoveredInfo; }
            private set
            {
                if( _discoveredInfo != value )
                {
                    _discoveredInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        /// <summary>
        /// Sets the discovery information that describes available plugins and services.
        /// If <see cref="IYodiiEngineExternal.IsRunning"/> is true, this can be rejected: the result will indicate the reason of the failure.
        /// </summary>
        /// <param name="dicoveredInfo">New discovered information to work with. When null, <see cref="EmptyDiscoveredInfo.Empty"/> is set.</param>
        /// <returns>Engine operation result.</returns>
        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if( info == null ) info = EmptyDiscoveredInfo.Empty;
            if( info == _discoveredInfo ) return Engine.SuccessResult;
            if( Engine.IsRunning )
            {
                var r = ConfigurationSolver.CreateAndApplyStaticResolution( Engine, FinalConfiguration, info, false, false, false );
                if( r.Item1 != null )
                {
                    Debug.Assert( !r.Item1.Success, "Not null means necessarily an error." );
                    Debug.Assert( r.Item1.Engine == this );
                    return r.Item1;
                }
                return Engine.DoDynamicResolution( r.Item2, null, null, () => DiscoveredInfo = info );
            }
            else DiscoveredInfo = info;
            return Engine.SuccessResult;
        }

        /// <summary>
        /// Gets a <see cref="YodiiConfiguration"/> object that is a copy of the current configuration.
        /// </summary>
        /// <returns>A new <see cref="YodiiConfiguration"/> object.</returns>
        public YodiiConfiguration GetConfiguration()
        {
            var c = new YodiiConfiguration();
            c.DiscoveredInfo = _discoveredInfo;
            foreach( var layer in Layers )
            {
                var l = new YodiiConfigurationLayer() { Name = layer.LayerName };
                foreach( var item in layer.Items )
                {
                    var i = new YodiiConfigurationItem() { ServiceOrPluginFullName = item.ServiceOrPluginFullName, Status = item.Status, Impact = item.Impact, Description = item.Description };
                    l.Items.Add( i );
                }
            }
            return c;
        }

        /// <summary>
        /// Sets a <see cref="YodiiConfiguration"/> object. Layers with the same name are merged by default (note that layers with a null, empty 
        /// or white space name are always into the default layer).
        /// This totally reconfigures the engine if it is possible.
        /// </summary>
        /// <param name="configuration">New configuration to apply. Can not be null.</param>
        /// <param name="mergeLayersWithSameName">False to keep multiple layers with the same name. By default layers with the same name are merged into one layer.</param>
        /// <returns>Yodii engine change result.</returns>
        public IYodiiEngineResult SetConfiguration( YodiiConfiguration configuration, bool mergeLayersWithSameName = true )
        {
            if( configuration == null ) throw new ArgumentNullException( "configuration" );
            IDiscoveredInfo newInfo = configuration.DiscoveredInfo ?? EmptyDiscoveredInfo.Empty;
            if( newInfo == _discoveredInfo ) newInfo = null;

            IEnumerable<KeyValuePair<string, Dictionary<string, YodiiConfigurationItem>>> rawLayers;
            Dictionary<string,Dictionary<string,YodiiConfigurationItem>> merged = null;
            List<KeyValuePair<string, Dictionary<string, YodiiConfigurationItem>>> unmerged = null;
            Dictionary<string, YodiiConfigurationItem> unmergedDefault = null;
            if( mergeLayersWithSameName )
            {
                merged = mergeLayersWithSameName ? new Dictionary<string, Dictionary<string, YodiiConfigurationItem>>() : null;
                rawLayers = merged;
            }
            else
            {
                unmerged = new List<KeyValuePair<string, Dictionary<string, YodiiConfigurationItem>>>();
                unmergedDefault = new Dictionary<string, YodiiConfigurationItem>();
                unmerged.Add( new KeyValuePair<string, Dictionary<string, YodiiConfigurationItem>>( String.Empty, unmergedDefault ) );
                rawLayers = unmerged;
            }
            foreach( var layer in configuration.Layers )
            {
                if( layer != null )
                {
                    string name = layer.Name;
                    if( String.IsNullOrWhiteSpace( name ) ) name = String.Empty;

                    Dictionary<string, YodiiConfigurationItem> mLayer;
                    if( mergeLayersWithSameName ) mLayer = merged.GetOrSet( name, n => new Dictionary<string, YodiiConfigurationItem>() );
                    else
                    {
                        if( name.Length == 0 ) mLayer = unmergedDefault;
                        else
                        {
                            mLayer = new Dictionary<string, YodiiConfigurationItem>();
                            unmerged.Add( new KeyValuePair<string, Dictionary<string, YodiiConfigurationItem>>( name, mLayer ) );
                        }
                    }
                    foreach( var item in layer.Items )
                    {
                        if( item != null )
                        {
                            YodiiConfigurationItem existing;
                            if( mLayer.TryGetValue( item.ServiceOrPluginFullName, out existing ) )
                            {
                                string invalidCombination;
                                ConfigurationStatus combinedStatus = FinalConfigurationItem.Combine( item.Status, existing.Status, out invalidCombination );
                                if( String.IsNullOrEmpty( invalidCombination ) )
                                {
                                    existing.Status = combinedStatus;
                                    existing.Impact = (item.Impact | existing.Impact).ClearUselessTryBits();
                                    if( item.Description.Length > existing.Description.Length ) existing.Description = item.Description;
                                }
                                else return new YodiiEngineResult( new ConfigurationFailureResult( String.Format( "Unable to merge Layer '{0}' for item '{1}': {2}", layer.Name, item.ServiceOrPluginFullName, invalidCombination ) ), Engine );
                            }
                            else mLayer.Add( item.ServiceOrPluginFullName, item );
                        }
                    }
                }
            }
            var finalConf = new Dictionary<string, FinalConfigurationItem>();
            ConfigurationFailureResult internalResult = FillFinalConfigDictionary( "SetConfiguration", rawLayers.SelectMany( l => l.Value.Values ), finalConf );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );
            
            IYodiiEngineResult result = OnConfigurationChanging( new ConfigurationChangingEventArgs( new FinalConfiguration( finalConf ), newInfo ) );
            if( !result.Success ) return result;
            Layers.OnSetConfiguration( rawLayers );
            OnConfigurationChanged();
            return Engine.SuccessResult;
        }

        ConfigurationFailureResult FillFromConfiguration( string currentOperation, Dictionary<string, FinalConfigurationItem> final, Func<IConfigurationItem, bool> filter = null )
        {
            var current = Layers.SelectMany( l => l.Items );
            return FillFinalConfigDictionary( currentOperation, filter == null ? current : current.Where( filter ), final );
        }

        ConfigurationFailureResult FillFinalConfigDictionary( string currentOperation, IEnumerable<IConfigurationItemData> items, Dictionary<string, FinalConfigurationItem> final )
        {
            foreach( var item in items )
            {
                if( item != null )
                {
                    FinalConfigurationItem data;
                    if( final.TryGetValue( item.ServiceOrPluginFullName, out data ) )
                    {
                        string invalidCombination;
                        ConfigurationStatus combinedStatus = FinalConfigurationItem.Combine( item.Status, data.Status, out invalidCombination );
                        if( String.IsNullOrEmpty( invalidCombination ) )
                        {
                            StartDependencyImpact combinedImpact = (data.Impact | item.Impact).ClearUselessTryBits();
                            final[item.ServiceOrPluginFullName] = new FinalConfigurationItem( item.ServiceOrPluginFullName, combinedStatus, combinedImpact );
                        }
                        else return new ConfigurationFailureResult( String.Format( "{0}: {1} for {2}", currentOperation, invalidCombination, item.ServiceOrPluginFullName ) );
                    }
                    else
                    {
                        final.Add( item.ServiceOrPluginFullName, new FinalConfigurationItem( item.ServiceOrPluginFullName, item.Status, item.Impact ) );
                    }
                }
            }
            return new ConfigurationFailureResult();
        }

        internal IYodiiEngineResult OnConfigurationItemChanging( ConfigurationItem item, FinalConfigurationItem data )
        {
            Debug.Assert( item != null && _finalConfiguration != null && Layers.Count != 0 );
            if( _currentEventArgs != null ) throw new InvalidOperationException( "Another change is in progress" );

            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( item.ServiceOrPluginFullName, data );

            ConfigurationFailureResult internalResult = FillFromConfiguration( "Item changing", final, c => c != item );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );

            FinalConfigurationChange status = FinalConfigurationChange.None;
            if( item.Status != data.Status ) status |= FinalConfigurationChange.StatusChanged;
            if( item.Impact != data.Impact ) status |= FinalConfigurationChange.ImpactChanged;

            return OnConfigurationChanging( new ConfigurationChangingEventArgs( new FinalConfiguration( final ), status, item ) );
        }

        internal IYodiiEngineResult OnConfigurationItemAdding( ConfigurationItem newItem )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( newItem.ServiceOrPluginFullName, new FinalConfigurationItem( newItem.ServiceOrPluginFullName, newItem.Status, newItem.Impact ));
          
            ConfigurationFailureResult internalResult = FillFromConfiguration( "Adding configuration item", final );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );

            return OnConfigurationChanging( new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.ItemAdded, newItem ) );
        }

        internal IYodiiEngineResult OnConfigurationItemRemoving( ConfigurationItem item )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c != item );
            Debug.Assert( internalResult.Success, "Removing a configuration item can not lead to an impossibility." );

            return OnConfigurationChanging( new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.ItemRemoved, item ) );
        }

        internal IYodiiEngineResult OnConfigurationLayerRemoving( ConfigurationLayer layer )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c.Layer != layer );
            Debug.Assert( internalResult.Success, "Removing a configuration layer can not lead to an impossibility." );

            return OnConfigurationChanging( new ConfigurationChangingEventArgs( new FinalConfiguration( final ), FinalConfigurationChange.LayerRemoved, layer ) );
        }

        IYodiiEngineResult OnConfigurationChanging( ConfigurationChangingEventArgs changingEvent )
        {
            if( Engine.IsRunning )
            {
                Tuple<IYodiiEngineStaticOnlyResult,ConfigurationSolver> t = Engine.StaticResolutionByConfigurationManager( changingEvent.NewDiscoveredInfo ?? _discoveredInfo, changingEvent.FinalConfiguration );
                if( t.Item1 != null )
                {
                    Debug.Assert( !t.Item1.Success );
                    Debug.Assert( t.Item1.Engine == Engine );
                    return t.Item1;
                }
                Action infoSetter = null;
                if( changingEvent.NewDiscoveredInfo != null ) infoSetter = () => DiscoveredInfo = changingEvent.NewDiscoveredInfo;
                return OnConfigurationChangingForExternalWorld( changingEvent ) ?? Engine.OnConfigurationChanging( t.Item2, infoSetter );
            }
            return OnConfigurationChangingForExternalWorld( changingEvent ) ?? Engine.SuccessResult;
        }

        internal IYodiiEngineResult OnConfigurationClearing()
        {
            Dictionary<string,FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            return OnConfigurationChanging( new ConfigurationChangingEventArgs( new FinalConfiguration() ) );
        }

        IYodiiEngineResult OnConfigurationChangingForExternalWorld( ConfigurationChangingEventArgs eventChanging )
        {
            _currentEventArgs = eventChanging;
            RaiseConfigurationChanging( _currentEventArgs );
            if( _currentEventArgs.IsCanceled )
            {
                return new YodiiEngineResult( new ConfigurationFailureResult( _currentEventArgs.FailureExternalReasons ), Engine );
            }
            return null;
        }

        internal void OnConfigurationChanged()
        {
            Debug.Assert( _currentEventArgs != null );

            FinalConfiguration = _currentEventArgs.FinalConfiguration;
            if( _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.StatusChanged
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ItemAdded
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ItemRemoved
                || _currentEventArgs.FinalConfigurationChange == FinalConfigurationChange.ImpactChanged)
            {
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationItemChanged ) );
            }
            else
            {
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationLayerChanged, _currentEventArgs.NewDiscoveredInfo ) );
            }
            _currentEventArgs = null;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged( [CallerMemberName] String propertyName = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion INotifyPropertyChanged

        private void RaiseConfigurationChanging( ConfigurationChangingEventArgs e )
        {
            var h = ConfigurationChanging;
            if( h != null ) h( this, e );
        }

        private void RaiseConfigurationChanged( ConfigurationChangedEventArgs e )
        {
            var h = ConfigurationChanged;
            if( h != null ) h( this, e );
        }

    }

}
