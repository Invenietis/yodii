using System;

namespace Yodii.Model
{
    /// <summary>
    /// This generic interface is automatically implemented for each <see cref="IYodiiService"/> and
    /// enables a plugin to manage service status.
    /// </summary>
    /// <typeparam name="T">The dynamic service interface.</typeparam>
    public interface IService<T> where T : IYodiiService
    {
        /// <summary>
        /// Gets the service itself. It is actually this object itself: <c>this</c> can be directly casted into 
        /// the interface.
        /// </summary>
        T Service { get; }

        /// <summary>
        /// Gets the current <see cref="ServiceStatus"/> of the service.
        /// </summary>
        ServiceStatus Status { get; }

        /// <summary>
        /// Fires whenever the <see cref="Status"/> changed.
        /// </summary>
        event EventHandler<ServiceStatusChangedEventArgs> ServiceStatusChanged;
    }
}
