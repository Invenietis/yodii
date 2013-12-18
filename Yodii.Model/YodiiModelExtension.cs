using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Extensions for Yodii interfaces.
    /// </summary>
    public static class YodiiModelExtension
    {

        /// <summary>
        /// </summary>
        /// <param name="this">This plugin information.</param>
        /// <returns>The final configuration status (considering the disabled case).</returns>
        public static ConfigurationStatus FinalConfigurationStatus( this IStaticSolvedPlugin @this )
        {
            return @this.DisabledReason != null ? ConfigurationStatus.Disabled : @this.WantedConfigSolvedStatus;
        }

        /// <summary>
        /// </summary>
        /// <param name="this">This service information.</param>
        /// <returns>The final configuration status (considering the disabled case).</returns>
        public static ConfigurationStatus FinalConfigurationStatus( this IStaticSolvedService @this )
        {
            return @this.DisabledReason != null ? ConfigurationStatus.Disabled : @this.WantedConfigSolvedStatus;
        }

    }
}
