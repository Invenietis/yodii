using System;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    /// <summary>
    /// Host for <see cref="IYodiiService"/> management.
    /// </summary>
    public interface IServiceHost
    {
        /// <summary>
        /// Gets a <see cref="ISimpleServiceHostConfiguration"/> that is always taken into account (one can not <see cref="Remove"/> it).
        /// Any change to it must be followed by a call to <see cref="ApplyConfiguration"/>.
        /// </summary>
        ISimpleServiceHostConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// Adds a configuration layer.
        /// The <see cref="ApplyConfiguration"/> must be called to actually update the 
        /// internal configuration.
        /// </summary>
        void Add( IServiceHostConfiguration configurator );

        /// <summary>
        /// Removes a configuration layer.
        /// The <see cref="ApplyConfiguration"/> must be called to actually update the 
        /// internal configuration.
        /// </summary>
        void Remove( IServiceHostConfiguration configurator );

        /// <summary>
        /// Applies the configuration: the <see cref="IServiceHostConfiguration"/> that have been <see cref="Add"/>ed are challenged
        /// for each intercepted method or event.
        /// </summary>
        void ApplyConfiguration();

        /// <summary>
        /// Gets the service implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the service (it can be a wrapped <see cref="IService{T}"/>).</param>
        /// <returns>The implementation or null if it does not exist.</returns>
        object GetProxy( Type interfaceType );

        /// <summary>
        /// Ensures that a proxy exists for the given interface and associates it to an implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="currentImplementation">Implementation to use.</param>
        /// <returns>The proxy object.</returns>
        object InjectExternalService( Type interfaceType, object currentImplementation );

        /// <summary>
        /// Ensures that a proxy exists for the given <see cref="IYodiiService"/> interface.
        /// </summary>
        /// <param name="interfaceType">Type of the interface that must extend <see cref="IYodiiService"/>.</param>
        /// <returns>The proxy object.</returns>
        object EnsureProxyForDynamicService( Type interfaceType );

        /// <summary>
        /// Ensures that a proxy exists for a dynamic service. The <see cref="IService{T}"/>.
        /// </summary>
        /// <param name="interfaceType">Type of the service (it can be a wrapped <see cref="IService{T}"/>).</param>
        /// <returns>The proxy to the service or null if it does not exist.</returns>
        IService<T> EnsureProxyForDynamicService<T>() where T : IYodiiService;

    }
}
