using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Configuration status for a configuration item (a plugin or a service).
    /// </summary>
    public enum ConfigurationStatus
    {
        /// <summary>
        /// Item must be disabled, and cannot be running.
        /// </summary>
        Disabled = -1,

        /// <summary>
        /// Item can be running or not. This is the default.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// Item must be able to start as needed.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// Item must be running.
        /// </summary>
        Running = 6
    }
}
