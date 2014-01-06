using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ConfigurationLayer : IConfigurationLayer
    {
        #region fields

        readonly ConfigurationItemCollection _configurationItemCollection;

        string _layerName;
        ConfigurationManager _owner;

        #endregion fields

        internal ConfigurationLayer( ConfigurationManager owner, string layerName )
        {
            _owner = owner;
            _layerName = String.IsNullOrWhiteSpace( layerName ) ? String.Empty : layerName;
            _configurationItemCollection = new ConfigurationItemCollection( this );
        }

        #region properties

        public string LayerName
        {
            get { return _layerName; }
            set
            {
                if ( String.IsNullOrWhiteSpace( value ) ) value = String.Empty;
                if ( _layerName != value )
                {
                    _layerName = value;
                    if ( _owner != null ) _owner.OnLayerNameChanged( this );
                    NotifyPropertyChanged();
                }
            }
        }

        public IConfigurationItemCollection Items
        {
            get { return _configurationItemCollection; }
        }

        IConfigurationManager IConfigurationLayer.ConfigurationManager
        {
            get { return _owner; }
        }

        public ConfigurationManager ConfigurationManager
        {
            get { return _owner; }
        }

        internal void SetConfigurationManager( ConfigurationManager c )
        {
            Debug.Assert( c != _owner );
            _owner = c;
            NotifyPropertyChanged();
        }

        #endregion properties

        internal IYodiiEngineResult OnConfigurationItemChanging( ConfigurationItem item, FinalConfigurationItem data )
        {
            Debug.Assert( item != null && item.Layer == this && _configurationItemCollection.Items.Contains( item ) );

            if( _owner == null ) return SuccessYodiiEngineResult.NullEngineSuccessResult;
            return _owner.OnConfigurationItemChanging( item, data );
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName]string propertyName = "" )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion

        public class ConfigurationItemCollection : IConfigurationItemCollection
        {
            CKObservableSortedArrayKeyList<ConfigurationItem, string> _items;
            ConfigurationLayer _layer;

            internal CKObservableSortedArrayKeyList<ConfigurationItem, string> Items
            {
                get { return _items; }
            }

            internal ConfigurationItemCollection( ConfigurationLayer parent )
            {
                _items = new CKObservableSortedArrayKeyList<ConfigurationItem, string>( e => e.ServiceOrPluginFullName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ) );
                _layer = parent;
                _items.PropertyChanged += RetrievePropertyEvent;
                _items.CollectionChanged += RetrieveCollectionEvent;
            }

            private void RetrieveCollectionEvent( object sender, NotifyCollectionChangedEventArgs e )
            {
                FireCollectionChanged( e );
            }

            private void RetrievePropertyEvent( object sender, PropertyChangedEventArgs e )
            {
                FirePropertyChanged( e );
            }

            public IYodiiEngineResult Add( string serviceOrPluginFullName, ConfigurationStatus status, string statusReason = "", StartDependencyImpact impact = StartDependencyImpact.Unknown)
            {
                if( String.IsNullOrEmpty( serviceOrPluginFullName ) ) throw new ArgumentException( "serviceOrPluginFullName is null or empty" );

                ConfigurationItem existing = _items.GetByKey( serviceOrPluginFullName );
                if( existing != null )
                {
                    IYodiiEngineResult res = existing.SetStatus( status );
                    if( res.Success ) return existing.SetImpact(impact);
                    return res;
                }

                ConfigurationItem newItem = new ConfigurationItem( _layer, serviceOrPluginFullName, status, impact, statusReason );
                if( _layer._owner == null )
                {
                    _items.Add( newItem );
                    return SuccessYodiiEngineResult.NullEngineSuccessResult;
                }

                IYodiiEngineResult result = _layer._owner.OnConfigurationItemAdding( newItem );
                if( result.Success )
                {
                    _items.Add( newItem );
                    _layer._owner.OnConfigurationChanged();
                    return result;
                }
                newItem.OnRemoved();
                return result;
            }

            /// <summary>
            /// Removes a configuration for plugin or a service.
            /// </summary>
            /// <param name="serviceOrPluginFullName">The identifier.</param>
            /// <returns>Detailed result of the operation.</returns>
            public IYodiiEngineResult Remove( string serviceOrPluginFullName )
            {
                if( String.IsNullOrEmpty( serviceOrPluginFullName ) ) throw new ArgumentException( "serviceOrPluginFullName is null or empty" );

                ConfigurationItem target = _items.GetByKey( serviceOrPluginFullName );
                if( target != null )
                {
                    if( _layer._owner == null )
                    {
                        target.OnRemoved();
                        _items.Remove( target );
                        return SuccessYodiiEngineResult.NullEngineSuccessResult;
                    }

                    IYodiiEngineResult result = _layer._owner.OnConfigurationItemRemoving( target );
                    if( result.Success )
                    {
                        target.OnRemoved();
                        _items.Remove( target );
                        _layer._owner.OnConfigurationChanged();
                        return result;
                    }
                    return result;
                }
                return new YodiiEngineResult( new ConfigurationFailureResult("Item not found"), _layer._owner != null ? _layer._owner.Engine : null );
            }

            public int Count
            {
                get { return _items.Count; }
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            private void FirePropertyChanged( PropertyChangedEventArgs e )
            {
                var h = PropertyChanged;
                if( h != null ) h( this, e );
            }

            private void FireCollectionChanged( NotifyCollectionChangedEventArgs e )
            {
                var h = CollectionChanged;
                if( h != null ) h( this, e );
            }

            public bool Contains( object item )
            {
                return _items.Contains( item );
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            IEnumerator<IConfigurationItem> IEnumerable<IConfigurationItem>.GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public int IndexOf( object item )
            {
                return _items.IndexOf( item );
            }

            public IConfigurationItem this[string key]
            {
                get { return this._items.GetByKey( key ); }
            }

            public IConfigurationItem this[int index]
            {
                get { return _items[index]; }
            }
        }
    }
}
