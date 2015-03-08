#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\YodiiModelExtension.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
        /// <param name="separator">The separator string.</param>
        /// <returns>The joined string.</returns>
        public static string Concatenate( this IEnumerable<string> @this, string separator = ", " )
        {
            return new StringBuilder().Append( @this, separator ).ToString();
        }

        /// <summary>
        /// This method can be used to dynamically start a service.
        /// There is no guaranty of success here: this is a deffered action that may not be applicable.
        /// </summary>
        /// <typeparam name="T">Actual type of the service to start.</typeparam>
        /// <param name="this">This event argument.</param>
        /// <param name="service">Reference to the service that should be started.</param>
        /// <param name="onSuccess">Optional action that will be executed when and if the service starts.</param>
        /// <param name="onError">Optional action that will be executed if the service has not been started.</param>
        public static void TryStart<T>( this ServiceStatusChangedEventArgs @this, IService<T> service, Action onSuccess = null, Action<IYodiiEngineResult> onError = null ) where T : IYodiiService
        {
            @this.TryStart( service, StartDependencyImpact.Unknown, onSuccess, onError );
        }

        /// <summary>
        /// This method can be used to dynamically start a service or a plugin.
        /// There is no guaranty of success here: this is a deffered action that may not be applicable.
        /// </summary>
        /// <param name="this">This event argument.</param>
        /// <param name="serviceOrPluginFullName">Full name of the service or plugin to start.</param>
        /// <param name="onSuccess">Optional action that will be executed when and if the service starts.</param>
        /// <param name="onError">Optional action that will be executed if the service has not been started.</param>
        public static void TryStart( this ServiceStatusChangedEventArgs @this, string serviceOrPluginFullName, Action onSuccess = null, Action<IYodiiEngineResult> onError = null )
        {
            @this.TryStart( serviceOrPluginFullName, StartDependencyImpact.Unknown, onSuccess, onError );
        }

    }
}
