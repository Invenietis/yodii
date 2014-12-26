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
        /// Attempts to start a plugin. 
        /// </summary>
        /// <param name="this">This engine.</param>
        /// <param name="pluginFullName">Name of the plugin to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability">.<see cref="ILiveRunCapability.CanStart"/>  property  
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The plugin must exist.</exception>
        /// <returns>Result detailing whether the plugin was successfully started or not.</returns>
        public static IYodiiEngineResult StartPlugin( this IYodiiEngineBase @this, string pluginFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            var p = @this.LiveInfo.FindPlugin( pluginFullName );
            if( p == null ) throw new ArgumentException();
            return @this.Start( p, impact );
        }

        /// <summary>
        /// Attempts to start a service. 
        /// </summary>
        /// <param name="this">This engine.</param>
        /// <param name="serviceFullName">Name of the service to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability">.<see cref="ILiveRunCapability.CanStart"/>  property  
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The service must exist.</exception>
        /// <returns>Result detailing whether the service was successfully started or not.</returns>
        public static IYodiiEngineResult StartService( this IYodiiEngineBase @this, string serviceFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            var s = @this.LiveInfo.FindService( serviceFullName );
            if( s == null ) throw new ArgumentException();
            return @this.Start( s, impact );
        }

        /// <summary>
        /// Attempts to stop a service or a plugin. 
        /// </summary>
        /// <param name="this">This engine.</param>
        /// <param name="pluginFullName">Name of the plugin to stop.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStop"/>' property must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The plugin must exist.</exception>
        /// <returns>Result detailing whether the service or plugin was successfully stopped or not.</returns>
        public static IYodiiEngineResult StopPlugin( this IYodiiEngineBase @this, string pluginFullName )
        {
            var p = @this.LiveInfo.FindPlugin( pluginFullName );
            if( p == null ) throw new ArgumentException();
            return @this.Stop( p );
        }

        /// <summary>
        /// Attempts to stop a service. 
        /// </summary>
        /// <param name="this">This engine.</param>
        /// <param name="serviceFullName">Name of the service to stop.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStop"/>' property must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The service must exist.</exception>
        /// <returns>Result detailing whether the service was successfully stopped or not.</returns>
        public static IYodiiEngineResult StopService( this IYodiiEngineBase @this, string serviceFullName )
        {
            var s = @this.LiveInfo.FindService( serviceFullName );
            if( s == null ) throw new ArgumentException();
            return @this.Stop( s );
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.Started"/> or <see cref="ServiceStatus.StartedSwapped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is started.</returns>
        public static bool IsStarted( this ServiceStatus @this )
        {
            return @this == ServiceStatus.Started || @this == ServiceStatus.StartedSwapped;
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.StartingSwapped"/> or <see cref="ServiceStatus.StoppingSwapped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is swapping.</returns>
        public static bool IsSwapping( this ServiceStatus @this )
        {
            return @this == ServiceStatus.StartingSwapped || @this == ServiceStatus.StoppingSwapped;
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.Stopped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is stopped.</returns>
        public static bool IsStopped( this ServiceStatus @this )
        {
            return @this == ServiceStatus.Stopped;
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.Started"/>, <see cref="ServiceStatus.StartedSwapped"/>,
        /// <see cref="ServiceStatus.Starting"/> or <see cref="ServiceStatus.StartingSwapped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is starting or started.</returns>
        public static bool IsStartingOrStarted( this ServiceStatus @this )
        {
            return (@this & (ServiceStatus)ServiceStatusValues.IsStart) != 0;
        }

        /// <summary>
        /// Gets whether this status is <see cref="ServiceStatus.Stopped"/>, <see cref="ServiceStatus.Stopping"/> or <see cref="ServiceStatus.StoppingSwapped"/>.
        /// </summary>
        /// <param name="this">This <see cref="ServiceStatus"/>.</param>
        /// <returns>True if the service is stopping or stopped.</returns>
        public static bool IsStoppingOrStopped( this ServiceStatus @this )
        {
            return (@this & (ServiceStatus)ServiceStatusValues.Stopped) != 0;
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

        /// <summary>
        /// Appends a set of strings with an internal separator.
        /// </summary>
        /// <param name="this">The <see cref="StringBuilder"/> to append to.</param>
        /// <param name="strings">Set of strings.</param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The builder itself.</returns>
        public static StringBuilder Append( this StringBuilder @this, IEnumerable<string> strings, string separator = ", " )
        {
            using( var e = strings.GetEnumerator() )
            {
                if( e.MoveNext() )
                {
                    @this.Append( e.Current );
                    while( e.MoveNext() )
                    {
                        @this.Append( separator ).Append( e.Current );
                    }
                }
            }
            return @this;
        }

        /// <summary>
        /// Concatenates multiple strings with an internal separator.
        /// </summary>
        /// <param name="this">Set of strings.</param>
        /// <param name="separator"></param>
        /// <param name="separator">The separator string.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<string> @this, string separator = ", " )
        {
            return new StringBuilder().Append( @this, separator ).ToString();
        }

        public static void TryStart<T>( this ServiceStatusChangedEventArgs @this, IService<T> service, Action<T> onStarted ) where T : IYodiiService
        {
            @this.TryStart( service, StartDependencyImpact.Unknown, onStarted );
        }

    }
}
