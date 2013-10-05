using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    internal class ConfigurationItemCollection : IEnumerable
    {
        private Dictionary<string,ConfigurationItem> _items;
        private ConfigurationLayer _parent;

        internal Dictionary<string, ConfigurationItem> Items
        {
            get { return _items; }
        }

        internal ConfigurationItemCollection( ConfigurationLayer parent )
        {
            _items = new Dictionary<string, ConfigurationItem>();
            _parent = parent;
        }

        public bool AddConfigurationItem( string serviceOrPluginName, ConfigurationStatus status )
        {
            if( string.IsNullOrEmpty( serviceOrPluginName ) ) throw new ArgumentNullException( "serviceOrPluginName is null" );

            //the layer is in a manager
            if( _parent != null )
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
            if( _items.ContainsKey( serviceOrPluginName ) )
            {
                if( _items[serviceOrPluginName].Status != status )
                {
                    //ConfigurationItem.SetStatus check if we can change the status
                    return _items[serviceOrPluginName].SetStatus( status );
                }
                return true;
            }
            else
            {
                ConfigurationItem newConfigurationItem = new ConfigurationItem( serviceOrPluginName, status, _parent );
                if( _parent.ConfigurationManagerParent.OnConfigurationLayerChanged( newConfigurationItem, status ) )
                {
                    _items.Add( newConfigurationItem.ServiceOrPluginName, newConfigurationItem );
                    return true;
                }
                return false;
            }
        }

        private bool AddItemWithoutParent( string serviceOrPluginName, ConfigurationStatus status )
        {
            //if the service or plugin already exist, we update his status
            if( _items.ContainsKey( serviceOrPluginName ) )
            {
                if( _items[serviceOrPluginName].Status != status )
                {
                    if( _items[serviceOrPluginName].CanChangeStatus( status ) )
                    {
                        _items[serviceOrPluginName].Status = status;
                        return true;
                    }
                    return false;
                }
                return true;
            }
            else
            {
                _items.Add( serviceOrPluginName, new ConfigurationItem( serviceOrPluginName, status, _parent ) );
                return true;
            }
        }

        public ConfigurationItem this[string key]
        {
            get { return this._items[key]; }
        }

        public int Count
        {
            get { return this._items.Count; }
        }

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _items.GetEnumerator();
        }

        #endregion
    }
}
