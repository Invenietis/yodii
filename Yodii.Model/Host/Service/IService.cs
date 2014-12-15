using System;

namespace Yodii.Model
{
    /// <summary>
    /// This generic interface is automatically implemented for each <see cref="IYodiiService"/> and
    /// enables a plugin to manage service status.
    /// </summary>
    /// <typeparam name="T">The dynamic service interface.</typeparam>
    public interface IService<T> : IServiceUntyped where T : IYodiiService
    {
        /// <summary>
        /// Gets the service itself. It is actually this object itself: <c>this</c> can be directly casted into 
        /// the <typeparamref name="T"/> interface.
        /// </summary>
        new T Service { get; }
    }
}
