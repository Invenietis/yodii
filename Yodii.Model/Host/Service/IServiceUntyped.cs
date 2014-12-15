using System;

namespace Yodii.Model
{
    /// <summary>
    /// This interface (automatically implemented for each <see cref="IYodiiService"/>) is the 
    /// base, non generic type, of <see cref="IService{T}"/>.
    /// </summary>
    public interface IServiceUntyped
    {
        /// <summary>
        /// Gets the service itself.
        /// </summary>
        IYodiiService Service { get; }

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
