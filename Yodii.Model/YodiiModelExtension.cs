using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Extensions for Yodii interfaces.
    /// </summary>
    public static class YodiiModelExtension
    {

        /// <summary>
        /// </summary>
        /// <param name="this">This live plugin/service info.</param>
        /// <returns>The engine result.</returns>
        public static IYodiiEngineResult Start( this ILiveYodiiItem @this )
        {
            return @this.Start( null, StartDependencyImpact.Unknown );
        }

        /// <summary>
        /// </summary>
        /// <param name="this">This live plugin/service info.</param>
        /// <param name="callerKey">Caller key.</param>
        /// <returns>The engine result.</returns>
        public static IYodiiEngineResult Start( this ILiveYodiiItem @this, string callerKey )
        {
            return @this.Start( callerKey, StartDependencyImpact.Unknown );
        }

        /// <summary>
        /// </summary>
        /// <param name="this">This live plugin/service info.</param>
        /// <param name="impact">The impact.</param>
        /// <returns>The engine result.</returns>
        public static IYodiiEngineResult Start( this ILiveYodiiItem @this, StartDependencyImpact impact )
        {
            return @this.Start( null, impact );
        }

        /// <summary>
        /// </summary>
        /// <param name="this">This live plugin/service info.</param>
        /// <returns>The engine result.</returns>
        public static IYodiiEngineResult Stop( this ILiveYodiiItem @this )
        {
            return @this.Stop( null );
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.Started"/>, <see cref="ServiceStatus.StartedSwapped"/>,
        /// <see cref="ServiceStatus.Starting"/> or <see cref="ServiceStatus.StartingSwapped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is starting or started.</returns>
        public static bool IsStartingOrStarted( this ServiceStatus @this )
        {
            return (@this & ServiceStatus.IsStart) != 0;
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.Started"/> or <see cref="ServiceStatus.StartedSwapped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is running.</returns>
        public static bool IsStarted( this ServiceStatus @this )
        {
            return @this == ServiceStatus.Started || @this == ServiceStatus.StartedSwapped;
        }

        /// <summary>
        /// Gets whether this service is running.
        /// </summary>
        /// <param name="this">This service.</param>
        /// <returns>True if the service is started.</returns>
        public static bool IsStartingOrStarted<T>( this IService<T> @this ) where T : IYodiiService
        {
            return @this.Status.IsStartingOrStarted();
        }

        public static void TryStart<T>( this ServiceStatusChangedEventArgs @this, IService<T> service, Action<T> onStarted ) where T : IYodiiService
        {
            @this.TryStart( service, StartDependencyImpact.Unknown, onStarted );
        }

    }
}
