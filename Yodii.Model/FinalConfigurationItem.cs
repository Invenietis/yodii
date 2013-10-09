using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public class FinalConfigurationItem : IConfigurationItem
    {
        private string _serviceOrPluginName;
        private ConfigurationStatus _status;
        private string _statusReason;

        public string ServiceOrPluginName
        {
            get { return _serviceOrPluginName; }
        }

        public ConfigurationStatus Status
        {
            get { return _status; }
            internal set { _status = value; }
        }

        public string StatusReason
        {
            get { return _statusReason; }
            set
            {
                _statusReason = value;
            }
        }

        internal FinalConfigurationItem( string serviceOrPluginName, ConfigurationStatus status, string statusReason = "" )
        {
            if( string.IsNullOrEmpty( serviceOrPluginName ) ) throw new ArgumentException( "serviceOrPluginID is null or empty" );

            _serviceOrPluginName = serviceOrPluginName;
            _status = status;
            _statusReason = statusReason;
        }

        internal FinalConfigurationItem( ConfigurationItem configurationItem )
            : this(configurationItem.ServiceOrPluginName, configurationItem.Status, configurationItem.StatusReason)
        {
        }

        #region IConfigurationItem Members

        public bool CanChangeStatus( ConfigurationStatus newStatus )
        {
            return (_status == ConfigurationStatus.Runnable) ? newStatus == ConfigurationStatus.Running :
                _status != ConfigurationStatus.Disable && _status != ConfigurationStatus.Running;
        }

        #endregion
    }
}
