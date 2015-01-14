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
                    ConfigurationManager.OnConfigurationChanged();
                }
                return result;
            }
            else
            {
                base.Remove( l );
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
                var prev = Store;
                Store = new ConfigurationLayer[] { _default };
                for( int i = 0; i < StoreCount; ++i )
                {
                    ConfigurationLayer l = (ConfigurationLayer)prev[i];
                    if( l == _default ) l.ClearDefaultLayer();
                    else l.Owner = null;
                }
                StoreCount = 1;
                RaiseReset();
                if( c != null ) c.OnConfigurationChanged();
            }
            return result;
        }

        IReadOnlyCollection<IConfigurationLayer> IConfigurationLayerCollection.this[string layerName]
        {
            get { return GetAllByKey( String.IsNullOrWhiteSpace( layerName ) ? String.Empty : layerName ); }
        }
    }
}
