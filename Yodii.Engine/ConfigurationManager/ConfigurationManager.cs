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
        FinalConfiguration _finalConfiguration;
        IDiscoveredInfo _discoveredInfo;

        ConfigurationChangingEventArgs _currentEventArgs;

        public event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;
        public event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        IConfigurationLayerCollection IConfigurationManager.Layers
        {
            get { return Layers; }
        }

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
            private set
            {
                _finalConfiguration = value;
                RaisePropertyChanged();
            }
        }

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

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if( info == null ) throw new ArgumentNullException( "info" );
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

        internal ConfigurationManager( YodiiEngine engine )
        {
            Engine = engine;
            Layers = new ConfigurationLayerCollection( this );
            _finalConfiguration = new FinalConfiguration();
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
        }

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

        public readonly YodiiEngine Engine;

        ConfigurationFailureResult FillFromConfiguration( string currentOperation, Dictionary<string, FinalConfigurationItem> final, Func<ConfigurationItem, bool> filter = null )
        {
            foreach( ConfigurationLayer layer in Layers )
            {
                ConfigurationStatus combinedStatus;
                string invalidCombination;

                foreach( ConfigurationItem item in layer.Items )
                {
                    if( filter == null || filter( item ) )
                    {
                        FinalConfigurationItem data;
                        if( final.TryGetValue( item.ServiceOrPluginFullName, out data ) )
                        {
                            combinedStatus = FinalConfigurationItem.Combine( item.Status, data.Status, out invalidCombination );
                            if( string.IsNullOrEmpty( invalidCombination ) )
                            {
                                StartDependencyImpact combinedImpact = (data.Impact|item.Impact).ClearUselessTryBits();
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

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, status, item ) );
        }

        internal IYodiiEngineResult OnConfigurationItemAdding( ConfigurationItem newItem )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            final.Add( newItem.ServiceOrPluginFullName, new FinalConfigurationItem( newItem.ServiceOrPluginFullName, newItem.Status, newItem.Impact ));
          
            ConfigurationFailureResult internalResult = FillFromConfiguration( "Adding configuration item", final );
            if( !internalResult.Success ) return new YodiiEngineResult( internalResult, Engine );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.ItemAdded, newItem ) );
        }

        internal IYodiiEngineResult OnConfigurationItemRemoving( ConfigurationItem item )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c != item );
            Debug.Assert( internalResult.Success, "Removing a configuration item can not lead to an impossibility." );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.ItemRemoved, item ) );
        }

        internal IYodiiEngineResult OnConfigurationLayerRemoving( ConfigurationLayer layer )
        {
            Dictionary<string, FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();

            ConfigurationFailureResult internalResult = FillFromConfiguration( null, final, c => c.Layer != layer );
            Debug.Assert( internalResult.Success, "Removing a configuration layer can not lead to an impossibility." );

            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf, FinalConfigurationChange.LayerRemoved, layer ) );
        }

        IYodiiEngineResult OnConfigurationChanging( Dictionary<string, FinalConfigurationItem> final, Func<FinalConfiguration, ConfigurationChangingEventArgs> createChangingEvent )
        {
            FinalConfiguration finalConfiguration = new FinalConfiguration( final );
            if( Engine.IsRunning )
            {
                Tuple<IYodiiEngineStaticOnlyResult,ConfigurationSolver> t = Engine.StaticResolutionByConfigurationManager( _discoveredInfo, finalConfiguration );
                if( t.Item1 != null )
                {
                    Debug.Assert( !t.Item1.Success );
                    Debug.Assert( t.Item1.Engine == Engine );
                    return t.Item1;
                }
                return OnConfigurationChangingForExternalWorld( createChangingEvent( finalConfiguration ) ) ?? Engine.OnConfigurationChanging( t.Item2 );
            }
            return OnConfigurationChangingForExternalWorld( createChangingEvent( finalConfiguration ) ) ?? Engine.SuccessResult;
        }

        internal IYodiiEngineResult OnConfigurationClearing()
        {
            Dictionary<string,FinalConfigurationItem> final = new Dictionary<string, FinalConfigurationItem>();
            return OnConfigurationChanging( final, finalConf => new ConfigurationChangingEventArgs( finalConf ) );
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
                RaiseConfigurationChanged( new ConfigurationChangedEventArgs( FinalConfiguration, _currentEventArgs.FinalConfigurationChange, _currentEventArgs.ConfigurationLayerChanged ) );
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
