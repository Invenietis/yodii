using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Bit flags that define the status for a service with its transition states. 
    /// </summary>
    [Flags]
    public enum ServiceStatus
    {
        None = 0,

        /// <summary>
        /// Bit that denotes a replaced implementation.
        /// </summary>
        IsSwap = 8,

        /// <summary>
        /// The service is currently stopped.
        /// </summary>
        Stopped = 1,

        /// <summary>
        /// Bis that flags a running service.
        /// When the implementation has been swapped (i.e. <see cref="Swapping"/> was the previous status instead of <see cref="Starting"/>), 
        /// it is <see cref="StartedSwapped"/>.
        /// </summary>
        IsStarted = 2,

        /// <summary>
        /// The service is currently running with and was previoulsy <see cref="Stopped"/>.
        /// When the implementation has been swapped (i.e. <see cref="Swapping"/> was the previous status instead of <see cref="Starting"/>), 
        /// the <see cref="ServiceStatusChangedEventArgs.Swap"/> property is true.
        /// </summary>
        Started = IsStarted,

        /// <summary>
        /// The service is currently running (and its implementation has been swapped).
        /// </summary>
        StartedSwapped = IsStarted | IsSwap,

        /// <summary>
        /// Bit that denotes a transition: either <see cref="Stopping"/>, <see cref="Starting"/> or <see cref="Swapping"/>.
        /// </summary>
        IsTransition = 32,

        /// <summary>
        /// The service is stopping.
        /// </summary>
        Stopping = Stopped | IsTransition,

        /// <summary>
        /// The service is starting.
        /// </summary>
        Starting = Started | IsTransition,

        /// <summary>
        /// The service is swapping its implementation.
        /// Current service implementation is the plugin that is stopping.
        /// Calling the <see cref="ServiceStatusChangedEventArgs.BindToStartingPlugin"/> method of the event argument
        /// bind the service to the new starting plugin.
        /// </summary>
        Swapping = Starting | IsSwap,


    }
}
