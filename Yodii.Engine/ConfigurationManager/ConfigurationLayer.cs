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
    public class ConfigurationLayer : IConfigurationLayer
    {
        #region fields

        readonly ConfigurationItemCollection _configurationItemCollection;

        string _layerName;
        ConfigurationManager _owner;

        #endregion fields

        #region constructors

        public ConfigurationLayer()
        {
            _layerName = string.Empty;
            _configurationItemCollection = new ConfigurationItemCollection( this );
        }

        public ConfigurationLayer( string configurationName )
            : this()
        {
            if( configurationName == null ) throw new ArgumentNullException( "configurationName" );

            _layerName = configurationName;
        }

        #endregion constructors

        #region properties

        public string LayerName
        {
            get { return _layerName; }
            set
            {
                if( value == null ) throw new ArgumentNullException();
                _layerName = value;
                if( _owner != null ) _owner.OnLayerNameChanged(this);
                NotifyPropertyChanged();
            }
        }

        public ConfigurationItemCollection Items
        {
            get { return _configurationItemCollection; }
        }

        public ConfigurationManager ConfigurationManager
        {
            get { return _owner; }
            internal set 
            { 
                _owner = value;
                NotifyPropertyChanged();
            }
        }

        #endregion properties

        internal IYodiiEngineResult OnConfigurationItemChanging( ConfigurationItem item, ConfigurationStatus newStatus )
        {
            Debug.Assert( item != null && item.Layer == this && _configurationItemCollection.Items.Contains( item ) );

            if( _owner == null ) return new SuccessYodiiEngineResult();
            return _owner.OnConfigurationItemChanging( item, newStatus );
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
            private ConfigurationLayer _layer;

            internal CKObservableSortedArrayKeyList<ConfigurationItem, string> Items
            {
                get { return _items; }
            }

            internal ConfigurationItemCollection( ConfigurationLayer parent )
            {
                _items = new CKObservableSortedArrayKeyList<ConfigurationItem, string>( e => e.ServiceOrPluginId, ( x, y ) => StringComparer.Ordinal.Compare( x, y ) );
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

            public IYodiiEngineResult Add( string serviceOrPluginId, ConfigurationStatus status, string statusReason = "" )
            {
                if( String.IsNullOrEmpty( serviceOrPluginId ) ) throw new ArgumentException( "serviceOrPluginId is null or empty" );

                ConfigurationItem existing = _items.GetByKey( serviceOrPluginId );
                if( existing != null ) return existing.SetStatus( status );

                ConfigurationItem newItem = new ConfigurationItem( _layer, serviceOrPluginId, status, statusReason );
                if( _layer._owner == null )
                {
                    _items.Add( newItem );
                    return new SuccessYodiiEngineResult();
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
            /// Removes a configuration.
            /// </summary>
            /// <param name="serviceOrPluginId">The identifier.</param>
            /// <returns>True if the identifier has actually been removed.</returns>
            public IYodiiEngineResult Remove( string serviceOrPluginId )
            {
                if( String.IsNullOrEmpty( serviceOrPluginId ) ) throw new ArgumentException( "serviceOrPluginId is null or empty" );

                ConfigurationItem target = _items.GetByKey( serviceOrPluginId );
                if( target != null )
                {
                    if( _layer._owner == null )
                    {
                        target.OnRemoved();
                        _items.Remove( target );
                        return new SuccessYodiiEngineResult();
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
                return new YodiiEngineResult( new ConfigurationFailureResult("Item not found") );
            }

            public ConfigurationItem this[string key]
            {
                get { return this._items.GetByKey( key ); }
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

            IEnumerator<ConfigurationItem> IEnumerable<ConfigurationItem>.GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public int IndexOf( object item )
            {
                return _items.IndexOf( item );
            }

            public ConfigurationItem this[int index]
            {
                get { return _items[index]; }
            }
        }
    }
}
