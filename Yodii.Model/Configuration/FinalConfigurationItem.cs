using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public struct FinalConfigurationItem
    {
        readonly string _serviceOrPluginId;
        readonly ConfigurationStatus _status;

        public string ServiceOrPluginId
        {
            get { return _serviceOrPluginId; }
        }

        public ConfigurationStatus Status
        {
            get { return _status; }
        }

        internal FinalConfigurationItem( string serviceOrPluginId, ConfigurationStatus status )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginId ) );

            _serviceOrPluginId = serviceOrPluginId;
            _status = status;
        }
    }
}
