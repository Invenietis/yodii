using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Defines the status for a service with its transition states. 
    /// </summary>
    public enum ServiceStatus
    {
        /// <summary>
        /// The service is currently disabled.
        /// </summary>
        Disabled = 0,

        /// <summary>
        /// The service is currently stopped.
        /// </summary>
        Stopped = 1,

        /// <summary>
        /// The service is stopping.
        /// </summary>
        Stopping = 2,

        /// <summary>
        /// The service is starting.
        /// </summary>
        Starting = 3,

        /// <summary>
        /// The service is currently running.
        /// When the implementation has been swapped (i.e. <see cref="Swapping"/> was the previous status instead of <see cref="Starting"/>), 
        /// the <see cref="ServiceStatusChangedEventArgs.Swap"/> property is true.
        /// </summary>
        Started = 4,

        /// <summary>
        /// The service is swapping its implementation.
        /// Current service implementation is the plugin that is stopping.
        /// Calling the <see cref="ServiceStatusChangedEventArgs.BindToStartingPlugin"/> method of the event argument
        /// bind the service to the new starting plugin.
        /// </summary>
        Swapping = 5
    }
}
