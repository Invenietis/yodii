#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationManager\ConfigurationLayerCollection.cs) is part of CiviKey. 
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
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    class ConfigurationLayerCollection : CKObservableSortedArrayKeyList<IConfigurationLayer, string>, IConfigurationLayerCollection
    {
        internal readonly ConfigurationManager ConfigurationManager;
        readonly ConfigurationLayer _default;

        public ConfigurationLayerCollection( ConfigurationManager parent )
            : base( e => e.LayerName, StringComparer.Ordinal.Compare, allowDuplicates: true )
        {
            ConfigurationManager = parent;
            _default = new ConfigurationLayer( this, String.Empty );
            Add( _default );
        }

        protected override void RaiseReset()
        {
            Debug.Assert( StoreCount == 0 || Store[0] == _default );
            if( StoreCount == 0 )
            {
                Store[0] = _default;
                StoreCount = 1;
            }
            base.RaiseReset();
        }

        public IConfigurationLayer Default
        {
            get { return _default; }
        }

        public IConfigurationLayer Create( string layerName )
        {
            if( String.IsNullOrEmpty( layerName ) ) throw new ArgumentException( "layerName" );
            var layer = new ConfigurationLayer( this, layerName );
            Add( layer );
            return layer;
        }

        public new IYodiiEngineResult Remove( IConfigurationLayer layer )
        {
            if( layer == null ) throw new ArgumentNullException( "layer" );
            // When called by a hacker.
            ConfigurationLayer l = layer as ConfigurationLayer;
            if( l == null ) throw new ArgumentException( "Invalid layer.", "layer" );
            if( l.LayerName.Length == 0 ) throw new ArgumentException( "Default layer can not be removed.", "layer" );

            if( ConfigurationManager != null )
            {
                if( layer.ConfigurationManager != ConfigurationManager ) return ConfigurationManager.Engine.SuccessResult;
                Debug.Assert( Contains( layer ), "Since layer.ConfigurationManager == _parent, then it necessarily belongs to us." );
                IYodiiEngineResult result = ConfigurationManager.OnConfigurationLayerRemoving( l );
                if( result.Success )
                {
                    base.Remove( l );
                    l.Detach();
                    ConfigurationManager.OnConfigurationChanged();
                }
                return result;
            }
            else
            {
                base.Remove( l );
                l.Detach();
                return SuccessYodiiEngineResult.NullEngineSuccessResult;
            }
        }

        public IConfigurationLayer this[string key]
        {
            get { return this.GetByKey( key ); }
        }

        public new IYodiiEngineResult Clear()
        {
            var c = ConfigurationManager;
            if( StoreCount == 1 ) return c != null ? c.Engine.SuccessResult : SuccessYodiiEngineResult.NullEngineSuccessResult;
            IYodiiEngineResult result = c != null ? c.OnConfigurationClearing() : SuccessYodiiEngineResult.NullEngineSuccessResult;
            if( result.Success )
            {
                RawClearContent( false );
                RaiseReset();
                if( c != null ) c.OnConfigurationChanged();
            }
            return result;
        }

        void RawClearContent( bool silentDefaultLayerClear )
        {
            var prev = Store;
            Store = new ConfigurationLayer[] { _default };
            for( int i = 0; i < StoreCount; ++i )
            {
                ConfigurationLayer l = (ConfigurationLayer)prev[i];
                if( l == _default ) l.Items.SilentClear( silentDefaultLayerClear );
                else l.Detach();
            }
            StoreCount = 1;
        }

        /// <summary>
        /// Called by the configuration manager when a successful SetConfiguration occurred.
        /// </summary>
        /// <param name="rawLayers">Configuration layers with their items.</param>
        internal void OnSetConfiguration( IEnumerable<KeyValuePair<string, Dictionary<string, YodiiConfigurationItem>>> rawLayers )
        {
            // This is not optimal but works: named layers are detached and the default one is silently cleared.
            // There could be a merge algorithm here but does it worth it? It is not that simple since it will have to decide how homonym layers must be reused...
            // Detached layers are cleared: their "ConfigurationManager" property is notified and their content is cleared so that observers are aware of the removal.
            // Since the default layer is silently cleared: Reset event will be raised when setting the new items or at the end.
            RawClearContent( true );
            bool defaultSet = false;
            var newOnes = new List<ConfigurationLayer>();
            newOnes.Add( _default );
            foreach( var rawLayer in rawLayers )
            {
                string name = rawLayer.Key;
                if( name.Length == 0 ) 
                {
                    _default.Items.OnSetConfiguration( rawLayer.Value.Values );
                    defaultSet = true;
                }
                else
                {
                    var conf = new ConfigurationLayer( this, name );
                    conf.Items.OnSetConfiguration( rawLayer.Value.Values );
                    newOnes.Add( conf );
                }
            }
            newOnes.Sort( ( x, y ) => StringComparer.Ordinal.Compare( x.LayerName, y.LayerName ) );
            Store = newOnes.ToArray();
            StoreCount = newOnes.Count;
            StoreVersion += 2;
            if( !defaultSet ) _default.Items.RaiseResetEvent();
            RaiseReset();
        }

        IReadOnlyCollection<IConfigurationLayer> IConfigurationLayerCollection.this[string layerName]
        {
            get { return GetAllByKey( String.IsNullOrWhiteSpace( layerName ) ? String.Empty : layerName ); }
        }
    }
}
