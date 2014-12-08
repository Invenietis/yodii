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
        /// When the implementation has been swapped (i.e. <see cref="StoppingSwapped"/> was the previous status instead of <see cref="Starting"/>), 
        /// it is <see cref="StartedSwapped"/>.
        /// </summary>
        IsStart = 2,

        /// <summary>
        /// The service is currently running with and was previoulsy <see cref="Stopped"/>.
        /// When the implementation has been swapped (i.e. <see cref="StartingSwapped"/> was the previous status instead of <see cref="Starting"/>), 
        /// the status is <see cref="StartedSwapped"/>.
        /// </summary>
        Started = IsStart,

        /// <summary>
        /// The service is currently running (and its implementation has been swapped).
        /// </summary>
        StartedSwapped = IsStart | IsSwap,

        /// <summary>
        /// Bit that denotes a transition: either <see cref="Stopping"/>, <see cref="StoppingSwapped"/>, <see cref="Starting"/> or <see cref="StartingSwapped"/>.
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
        /// Calling the <see cref="ServiceStatusChangedEventArgs.BindToSwappedPlugin"/> method of the event argument
        /// bind the service to the new starting plugin.
        /// </summary>
        StoppingSwapped = Stopping | IsSwap,

        /// <summary>
        /// The service is swapping its implementation.
        /// Current service implementation is the plugin that is starting.
        /// Calling the <see cref="ServiceStatusChangedEventArgs.BindToSwappedPlugin"/> method of the event argument
        /// bind the service to the previous (stopping) plugin.
        /// </summary>
        StartingSwapped = Starting | IsSwap,

    }
}
