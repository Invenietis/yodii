using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.ComponentModel;

namespace Yodii.Model
{
    //ToDo : removeItem, Obersable, ienumerable, 
    public class ConfigurationLayer
    {
        #region fields

        private string _configurationName;
        private ConfigurationItemCollection _configurationItemCollection;
        private ConfigurationManager _configurationManagerParent;

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

        internal ConfigurationManager ConfigurationManagerParent
        {
            get { return _configurationManagerParent; }
            set { _configurationManagerParent = value; }
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
            if ( configurationName == null ) throw new ArgumentNullException( "configurationName" );

            _configurationName = configurationName;
        }

        //use to create a finalConfigurationLayer
        internal ConfigurationLayer( string configurationName, IList<ConfigurationItem> items, ConfigurationManager parent )
        {
            _configurationName = configurationName;
            foreach ( ConfigurationItem item in items )
            {
                _configurationItemCollection.Items.Add( item.ServiceOrPluginName, item );
            }
            _configurationManagerParent = parent;
        }

        internal bool OnConfigurationItemChanged( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            _configurationManagerParent.OnConfigurationLayerChanged( configurationItem, newStatus );
            return true;
        }

        public class ConfigurationItemCollection : IEnumerable, INotifyCollectionChanged, INotifyPropertyChanged
        {
            CKObservableSortedArrayKeyList<ConfigurationItem, string> _items;
            private ConfigurationLayer _parent;

            internal CKObservableSortedArrayKeyList<ConfigurationItem, string> Items
            {
                get { return _items; }
            }

            internal ConfigurationItemCollection( ConfigurationLayer parent )
            {
                _items = new CKObservableSortedArrayKeyList<ConfigurationItem, string>( e => e.ServiceOrPluginName, ( x, y ) => StringComparer.Ordinal.Compare( x, y ) );
                _parent = parent;
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

            public bool AddConfigurationItem( string serviceOrPluginName, ConfigurationStatus status )
            {
                if ( string.IsNullOrEmpty( serviceOrPluginName ) ) throw new ArgumentNullException( "serviceOrPluginName is null" );

                //the layer is in a manager
                if ( _parent != null )
                {
                    return AddItemWithParent( serviceOrPluginName, status );
                }
                else
                {
                    return AddItemWithoutParent( serviceOrPluginName, status );
                }
            }

            public bool RemoveConfigurationItem( string serviceOrPluginName )
            {
                throw new NotImplementedException();
            }

            private bool AddItemWithParent( string serviceOrPluginName, ConfigurationStatus status )
            {
                //if the service or plugin already exist, we update his status
                if ( _items.Contains( serviceOrPluginName ) )
                {
                    if ( _items.GetByKey( serviceOrPluginName ).Status != status )
                    {
                        //ConfigurationItem.SetStatus check if we can change the status
                        return _items.GetByKey( serviceOrPluginName ).SetStatus( status );
                    }
                    return true;
                }
                else
                {
                    ConfigurationItem newConfigurationItem = new ConfigurationItem( serviceOrPluginName, status, _parent );
                    if ( _parent.ConfigurationManagerParent.OnConfigurationLayerChanged( newConfigurationItem, status ) )
                    {
                        _items.Add( newConfigurationItem );
                        return true;
                    }
                    return false;
                }
            }

            private bool AddItemWithoutParent( string serviceOrPluginName, ConfigurationStatus status )
            {
                //if the service or plugin already exist, we update his status
                if ( _items.Contains( serviceOrPluginName ) )
                {
                    if ( _items.GetByKey( serviceOrPluginName ).Status != status )
                    {
                        if ( _items.GetByKey( serviceOrPluginName ).CanChangeStatus( status ) )
                        {
                            _items.GetByKey( serviceOrPluginName ).Status = status;
                            return true;
                        }
                        return false;
                    }
                    return true;
                }
                else
                {
                    _items.Add( new ConfigurationItem( serviceOrPluginName, status, _parent ) );
                    return true;
                }
            }

            public ConfigurationItem this[string key]
            {
                get { return this._items.GetByKey( key ); }
            }

            public int Count
            {
                get { return this._items.Count; }
            }

            public event NotifyCollectionChangedEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;

            private void FirePropertyChanged( PropertyChangedEventArgs e )
            {
                if ( this.PropertyChanged != null )
                {
                    this.PropertyChanged( this, e );
                }
            }
            private void FireCollectionChanged( NotifyCollectionChangedEventArgs e )
            {
                if ( this.CollectionChanged != null )
                {
                    this.CollectionChanged( this, e );
                }
            }

            #region IEnumerable Members

            IEnumerator IEnumerable.GetEnumerator()
            {
                return _items.GetEnumerator();
            }
            #endregion
        }
    }
}
