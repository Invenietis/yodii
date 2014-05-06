using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Event argument when a service <see cref="RunningStatus">status</see> changed.
    /// This event is available on the generic <see cref="IService{T}"/>.<see cref="IService{T}.ServiceStatusChanged">ServiceStatusChanged</see>.
    /// </summary>
    public class ServiceStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previous status.
        /// </summary>
        public RunningStatus Previous { get; private set; }

        /// <summary>
        /// Gets the current status of the service.
        /// </summary>
        public RunningStatus Current { get; private set; }

        /// <summary>
        /// Initializes a new instance of a <see cref="ServiceStatusChangedEventArgs"/>.
        /// </summary>
        /// <param name="previous">The previous running status.</param>
        /// <param name="current">The current running Status</param>
        /// <param name="allowErrorTransition">True if the next status is a valid next one (like <see cref="RunningStatus.Starting"/> to <see cref="RunningStatus.Started"/>). False otherwise.</param>
        public ServiceStatusChangedEventArgs( RunningStatus previous, RunningStatus current, bool allowErrorTransition )
        {
            //Debug.Assert( previous.IsValidTransition( current, allowErrorTransition ) );
            Previous = previous;
            Current = current;
        }

    }
}
