using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Configuration status for a configuration item.
    /// </summary>
    public enum ConfigurationStatus
    {
        /// <summary>
        /// Item must be disabled, and cannot be run.
        /// </summary>
        Disabled = -1,

        /// <summary>
        /// Item can be started or disabled. Usually default.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// Item is not started must must allow for starting, and cannot be disabled.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// Item must run.
        /// </summary>
        Running = 6
    }
}
