using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using System.ComponentModel;

namespace Yodii.Model
{
    //ToDo : removeItem, Obersable, ienumerable, 
    public class ConfigurationLayer : INotifyPropertyChanged
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
            set
            {
                if( value == null ) throw new NullReferenceException();
                _configurationName = value;
                NotifyPropertyChanged();
            }
        }

        public ConfigurationItemCollection Items
        {
            get { return _configurationItemCollection; }
        }

        public ConfigurationManager ConfigurationManagerParent
        {
            get { return _configurationManagerParent; }
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

        internal ConfigurationResult OnConfigurationItemChanging( FinalConfigurationChange change, ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            return _configurationManagerParent.OnConfigurationLayerChanging( change, configurationItem, newStatus );
        }

        internal void OnConfigurationItemChanged( FinalConfigurationChange change, ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            _configurationManagerParent.OnConfigurationItemChanged( change, configurationItem, newStatus );
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName] String propertyName = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion INotifyPropertyChanged

        //SortedArrayList from CK.Core

        public class ConfigurationItemCollection : IEnumerable
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
            }

            public bool Add( string serviceOrPluginName, ConfigurationStatus status )
            {
                if( string.IsNullOrEmpty( serviceOrPluginName ) ) throw new ArgumentNullException( "serviceOrPluginName is null" );

                //the layer is in a manager
                if( _parent.ConfigurationManagerParent != null )
                {
                    return AddItemWithParent( serviceOrPluginName, status );
                }
                else
                {
                    return AddItemWithoutParent( serviceOrPluginName, status );
                }
            }

            public bool Remove( string serviceOrPluginName )
            {
                throw new NotImplementedException();
            }

            private bool AddItemWithParent( string serviceOrPluginName, ConfigurationStatus status )
            {
                //if the service or plugin already exist, we update his status
                if( _items.Contains( serviceOrPluginName ) )
                {
                    if( _items.GetByKey( serviceOrPluginName ).Status != status )
                    {
                        //ConfigurationItem.SetStatus check if we can change the status
                        return _items.GetByKey( serviceOrPluginName ).SetStatus( status );
                    }
                    return new ConfigurationResult();
                }
                else
                {
                    ConfigurationItem newConfigurationItem = new ConfigurationItem( serviceOrPluginName, status, _parent );
                    ConfigurationResult result = _parent.OnConfigurationItemChanging( FinalConfigurationChange.ItemAdded, newConfigurationItem, status );
                    if( result )
                    {
                        _items.Add( newConfigurationItem );
                        return new ConfigurationResult();
                    }
                    return result;
                }
            }

            private ConfigurationResult AddItemWithoutParent( string serviceOrPluginName, ConfigurationStatus status )
            {
                //if the service or plugin already exist, we update his status
                if( _items.Contains( serviceOrPluginName ) )
                {
                    ConfigurationItem item = _items.GetByKey( serviceOrPluginName );
                    if( item.Status != status )
                    {
                        if( item.CanChangeStatus( status ) )
                        {
                            item.Status = status;
                            return new ConfigurationResult();
                        }
                        return new ConfigurationResult( string.Format( "{0} already exists and cannot switch from {1} to {2}", item.ServiceOrPluginName, item.Status, status ) );
                    }
                    return new ConfigurationResult();
                }
                else
                {
                    if( _items.Add( new ConfigurationItem( serviceOrPluginName, status, _parent ) ) )
                    {
                        return new ConfigurationResult();
                    }
                    else
                    {
                        return new ConfigurationResult( "A problem has been encountered while adding the ConfigurationItem in the collection" );
                    }
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
            public event CollectionChangeEventHandler CollectionChanged;
            public event PropertyChangedEventHandler PropertyChanged;
            /*    
            _items.PropertyChanged += (s, e) 
            {

            };
            _items.CollectionChanged += (s, e) 
            {

            };
                */
            private void FirePropertyChanged( PropertyChangedEventArgs e )
            {
                if( this.PropertyChanged != null )
                {
                    this.PropertyChanged( this, e );
                }
            }
            private void FireCollectionChanged( CollectionChangeEventArgs e )
            {
                if( this.CollectionChanged != null )
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
