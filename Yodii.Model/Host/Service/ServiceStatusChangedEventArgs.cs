using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Event argument when a <see cref="IService{T}.Status"/> changed.
    /// This event is available on the generic <see cref="IService{T}"/>.<see cref="IService{T}.ServiceStatusChanged">ServiceStatusChanged</see>.
    /// </summary>
    public abstract class ServiceStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Plugins can call this method only when <see cref="IService{T}.Status"/> is <see cref="ServiceStatus.Swapping"/>.
        /// By calling this method, the <see cref="IService{T}"/> is bound to the new plugin that implements the service.
        /// </summary>
        public abstract void BindToStartingPlugin();

        /// <summary>
        /// This method can be used to dynamically start a service. There is no guaranty of success here: this is a deffered action
        /// that may not be applicable.
        /// </summary>
        /// <typeparam name="T">Actual type of the service to start.</typeparam>
        /// <param name="service">Reference to the service that should be started.</param>
        /// <param name="impact">Impact of the start.</param>
        /// <param name="onStarted">Action that will be executed when and if the service starts.</param>
        public abstract void TryStart<T>( IService<T> service, StartDependencyImpact impact, Action<T> onStarted ) where T : IYodiiService;

    }
}
