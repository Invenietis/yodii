using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    //ToDo : removeItem, Obersable
    public class ConfigurationLayer
    {
        #region fields

        private string _configurationName;
        private Dictionary<string,ConfigurationItem> _configurationItemCollection;
        private ConfigurationManager _configurationManagerParent;

        #endregion fields

        #region properties

        public string ConfigurationName
        {
            get { return _configurationName; }
        }
        public IReadOnlyList<ConfigurationItem> ConfigurationItemCollection
        {
            get { return _configurationItemCollection.Values.ToReadOnlyList(); }
        }
        internal ConfigurationManager ConfigurationManagerParent
        {
            get { return _configurationManagerParent; }
            set { _configurationManagerParent = value; }
        }

        #endregion properties

        public ConfigurationLayer( string configurationName )
        {
            if( string.IsNullOrEmpty( configurationName ) ) throw new ArgumentNullException( "configurationName is null" );

            _configurationName = configurationName;
            _configurationItemCollection = new Dictionary<string, ConfigurationItem>();
        }

        //use to create a finalConfigurationLayer
        internal ConfigurationLayer( string configurationName, IList<ConfigurationItem> items, ConfigurationManager parent )
        {
            _configurationName = configurationName;
            foreach( ConfigurationItem item in items )
            {
                _configurationItemCollection.Add( item.ServiceOrPluginName, item );
            }
            _configurationManagerParent = parent;
        }

        public bool AddConfigurationItem( string serviceOrPluginName, ConfigurationStatus status )
        {
            if( string.IsNullOrEmpty( serviceOrPluginName ) ) throw new ArgumentNullException( "serviceOrPluginName is null" );

            //the layer is in a manager
            if( _configurationManagerParent != null )
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

        internal bool OnConfigurationItemChanged( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            _configurationManagerParent.OnConfigurationLayerChanged( configurationItem, newStatus );
            return true;
        }

        private bool AddItemWithParent( string serviceOrPluginName, ConfigurationStatus status )
        {
            //if the service or plugin already exist, we update his status
            if( _configurationItemCollection.ContainsKey( serviceOrPluginName ) )
            {
                if( _configurationItemCollection[serviceOrPluginName].Status != status )
                {
                    //ConfigurationItem.SetStatus check if we can change the status
                    return _configurationItemCollection[serviceOrPluginName].SetStatus( status );
                }
                return true;
            }
            else
            {
                ConfigurationItem newConfigurationItem = new ConfigurationItem( serviceOrPluginName, status, this );
                if( _configurationManagerParent.OnConfigurationLayerChanged( newConfigurationItem, status ) )
                {
                    _configurationItemCollection.Add( newConfigurationItem.ServiceOrPluginName, newConfigurationItem );
                    return true;
                }
                return false;
            }
        }

        private bool AddItemWithoutParent( string serviceOrPluginName, ConfigurationStatus status )
        {
            //if the service or plugin already exist, we update his status
            if( _configurationItemCollection.ContainsKey( serviceOrPluginName ) )
            {
                if( _configurationItemCollection[serviceOrPluginName].Status != status )
                {
                    if( _configurationItemCollection[serviceOrPluginName].CanChangeStatus( status ) )
                    {
                        _configurationItemCollection[serviceOrPluginName].Status = status;
                        return true;
                    }
                    return false;
                }
                return true;
            }
            else
            {
                _configurationItemCollection.Add( serviceOrPluginName, new ConfigurationItem( serviceOrPluginName, status, this ) );
                return true;
            }
        }

        public ConfigurationItem this[string key]
        {
            get { return this._configurationItemCollection[key]; }
        }

        public int Count
        {
            get { return this._configurationItemCollection.Count; }
        }
    }
}
