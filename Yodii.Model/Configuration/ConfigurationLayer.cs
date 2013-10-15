using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.ComponentModel;
using System.Diagnostics;

namespace Yodii.Model
{
    //ToDo : removeItem, Obersable, ienumerable, 
    public class ConfigurationLayer
    {
        #region fields

        readonly ConfigurationItemCollection _configurationItemCollection;
        string _configurationName;
        ConfigurationManager _configurationManagerParent;

        #endregion fields

        #region properties

        public string ConfigurationName
        {
            get { return _configurationName; }
        }

        public ConfigurationItemCollection Items
        {
            get { return _configurationItemCollection; }
        }

        public ConfigurationManager ConfigurationManagerParent
        {
            get { return _configurationManagerParent; }
            internal set { _configurationManagerParent = value; }
        }

        #endregion properties

        public ConfigurationLayer()
        {
            _configurationName = string.Empty;
            _configurationItemCollection = new ConfigurationItemCollection( this );
        }

        public ConfigurationLayer( string configurationName )
            : this()
        {
            if( configurationName == null ) throw new ArgumentNullException( "configurationName" );
            _configurationName = configurationName;
        }

        internal bool OnConfigurationItemChanging( ConfigurationItem item, ConfigurationStatus newStatus )
        {
            Debug.Assert( item != null && item.Layer == this && _configurationItemCollection.Items.Contains( item ) );
            
            if( _configurationManagerParent == null ) return true;
            return _configurationManagerParent.OnConfigurationItemChanging( this, item, newStatus );
        }

        public class ConfigurationItemCollection : ICKObservableReadOnlyList<ConfigurationItem>
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

            public bool Add( string serviceOrPluginId, ConfigurationStatus status )
            {
                if( String.IsNullOrEmpty( serviceOrPluginId ) ) throw new ArgumentNullException( "serviceOrPluginId" );

                ConfigurationItem existing = _items.GetByKey( serviceOrPluginId );
                if( existing != null ) return existing.SetStatus( status );

                var newItem = new ConfigurationItem( _layer, serviceOrPluginId, status );
                if( _layer._configurationManagerParent == null )
                {
                    _items.Add( newItem );
                    _layer._configurationManagerParent.OnConfigurationItemAdded( _layer, newItem );
                    return true;
                }

                if( _layer._configurationManagerParent.OnConfigurationItemAdding( _layer, newItem ) )
                {
                    _items.Add( newItem );
                    _layer._configurationManagerParent.OnConfigurationItemAdded( _layer, newItem );
                    return true;
                }
                newItem.OnRemoved();
                return false;
            }

            /// <summary>
            /// Removes a configuration.
            /// </summary>
            /// <param name="serviceOrPluginId">The identifier.</param>
            /// <returns>True if the identifier has actually been removed.</returns>
            public bool Remove( string serviceOrPluginId )
            {
                if( String.IsNullOrEmpty( serviceOrPluginId ) ) throw new ArgumentNullException( "serviceOrPluginId" );

                throw new NotImplementedException();
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

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }

            public bool Contains( object item )
            {
                return _items.Contains( item );
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
