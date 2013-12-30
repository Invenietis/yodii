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
        readonly string _serviceOrPluginFullName;
        readonly ConfigurationStatus _status;
        readonly StartDependencyImpact _impact;

        /// <summary>
        /// Service or plugin ID.
        /// </summary>
        public string ServiceOrPluginFullName
        {
            get { return _serviceOrPluginFullName; }
        }

        /// <summary>
        /// Required configuration status.
        /// </summary>
        public ConfigurationStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Required configuration impact.
        /// </summary>
        public StartDependencyImpact Impact
        {
            get { return _impact; }
        }

        internal FinalConfigurationItem( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginFullName ) );

            _serviceOrPluginFullName = serviceOrPluginFullName;
            _status = status;
            _impact = impact;
        }
    }
}
