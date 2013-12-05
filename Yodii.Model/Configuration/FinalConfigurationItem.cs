using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Read-only configuration item.
    /// </summary>
    public struct FinalConfigurationItem
    {
        readonly string _serviceOrPluginId;
        readonly ConfigurationStatus _status;

        /// <summary>
        /// Service or plugin ID.
        /// </summary>
        public string ServiceOrPluginId
        {
            get { return _serviceOrPluginId; }
        }

        /// <summary>
        /// Required configuration status.
        /// </summary>
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
