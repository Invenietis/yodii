using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Event argument when a <see cref="IServiceUntyped.Status"/> changed.
    /// This event is available on the generic <see cref="IService{T}"/>.<see cref="IServiceUntyped.ServiceStatusChanged">ServiceStatusChanged</see>.
    /// </summary>
    public abstract class ServiceStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets whether implementation is swapping: <see cref="IServiceUntyped.Status"/> is <see cref="ServiceStatus.StoppingSwapped"/> or <see cref="ServiceStatus.StartingSwapped"/>.
        /// </summary>
        public abstract bool IsSwapping { get; }

        /// <summary>
        /// Plugins can call this method only when <see cref="IServiceUntyped.Status"/> is <see cref="ServiceStatus.StoppingSwapped"/> or <see cref="ServiceStatus.StartingSwapped"/>.
        /// By calling this method, the <see cref="IServiceUntyped"/> is bound to the strating or stopping plugin that implements the service.
        /// </summary>
        public abstract void BindToSwappedPlugin();

        /// <summary>
        /// This method can be used to dynamically start a service. 
        /// There is no guaranty of success here: this is a deffered action that may not be applicable.
        /// </summary>
        /// <typeparam name="T">Actual type of the service to start.</typeparam>
        /// <param name="service">Reference to the service that should be started.</param>
        /// <param name="impact">Impact of the start.</param>
        /// <param name="onSuccess">Optional action that will be executed when and if the service starts.</param>
        /// <param name="onError">Optional action that will be executed if the service has not been started.</param>
        public abstract void TryStart<T>( IService<T> service, StartDependencyImpact impact, Action onSuccess = null, Action<IYodiiEngineResult> onError = null ) where T : IYodiiService;

        /// <summary>
        /// This method can be used to dynamically start a service or a plugin. 
        /// There is no guaranty of success here: this is a deffered action that may not be applicable.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Full name of the service or plugin to start.</param>
        /// <param name="impact">Impact of the start.</param>
        /// <param name="onSuccess">Optional action that will be executed when and if the service starts.</param>
        /// <param name="onError">Optional action that will be executed if the service has not been started.</param>
        public abstract void TryStart( string serviceOrPluginFullName, StartDependencyImpact impact, Action onSuccess = null, Action<IYodiiEngineResult> onError = null );

    }
}
