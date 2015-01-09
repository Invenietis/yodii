#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Host\Service\ServiceStatusChangedEventArgs.cs) is part of CiviKey. 
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
