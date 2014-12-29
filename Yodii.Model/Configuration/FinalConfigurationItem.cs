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

        /// <summary>
        /// A FinalConfigurationItem is an immutable object that displays the latest configuration of an item.
        /// </summary>
        public FinalConfigurationItem( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginFullName ) );

            _serviceOrPluginFullName = serviceOrPluginFullName;
            _status = status;
            _impact = impact;
        }

        /// <summary>
        /// This function encapsulates all configuration constraints regarding the <see cref="ConfigurationStatus"/> of an item.
        /// </summary>
        /// <param name="s1">Current <see cref="ConfigurationStatus"/></param>
        /// <param name="s2">Wanted <see cref="ConfigurationStatus"/></param>
        /// <param name="invalidCombination">string expliciting the error. Empty if all is well</param>
        /// <returns></returns>
        public static ConfigurationStatus Combine( ConfigurationStatus s1, ConfigurationStatus s2, out string invalidCombination )
        {
            invalidCombination = "";
            if( s1 == s2 ) return s1;
           
            if( s2 != ConfigurationStatus.Optional )
            {
                if( s1 == ConfigurationStatus.Optional || (s1 >= ConfigurationStatus.Runnable && s2 >= ConfigurationStatus.Runnable) )
                {
                    return (s1 == ConfigurationStatus.Running) ? s1 : s2;
                }
                else if( s1 != s2 )
                {
                    invalidCombination = string.Format( "Conflict for statuses {0} and {1}", s1, s2 );
                    return s1;
                }
                else
                {
                    invalidCombination = string.Format( "Something went terribly, terribly wrong..." );
                    return s1;
                }
            }
            return s1;
        }
    }
}
