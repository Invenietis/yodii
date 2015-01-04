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
            if( StoreCount == 1 ) return ConfigurationManager.Engine.SuccessResult;
            IYodiiEngineResult result = ConfigurationManager != null ? ConfigurationManager.OnConfigurationClearing() : SuccessYodiiEngineResult.NullEngineSuccessResult;
            if( result.Success )
            {
                var prev = Store;
                Store = new ConfigurationLayer[] { _default };
                foreach( ConfigurationLayer l in prev )
                {
                    if( l == _default ) l.ClearDefaultLayer();
                    else l.Owner = null;
                }
                RaiseReset();
                if( ConfigurationManager != null ) ConfigurationManager.OnConfigurationChanged();
            }
            return result;
        }

        IReadOnlyCollection<IConfigurationLayer> IConfigurationLayerCollection.this[string layerName]
        {
            get { return GetAllByKey( String.IsNullOrWhiteSpace( layerName ) ? String.Empty : layerName ); }
        }
    }
}
