using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Defines the current status of a plugin (or a service).
    /// </summary>
    public enum RunningStatus
    {
        /// <summary>
        /// The plugin (or service) is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The plugin (or service) is stopped. It can be started.
        /// </summary>
        Stopped,

        /// <summary>
        /// The plugin (or service) is running. It can be stopped. 
        /// </summary>
        Running,

        /// <summary>
        /// The plugin (or service) is running and cannot be stopped. 
        /// </summary>
        RunningLocked
    }
}
