using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

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
                _configurationItemCollection.Items.Add( item.ServiceOrPluginName, item );
            }
            _configurationManagerParent = parent;
        }

        internal bool OnConfigurationItemChanged( ConfigurationItem configurationItem, ConfigurationStatus newStatus )
        {
            _configurationManagerParent.OnConfigurationLayerChanged( configurationItem, newStatus );
            return true;
        }
    }
}
