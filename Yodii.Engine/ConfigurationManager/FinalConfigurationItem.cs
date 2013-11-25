using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Engine
{
    public struct FinalConfigurationItem : Yodii.Model.Configuration.IFinalConfigurationItem
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
