#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\IYodiiEngineBase.cs) is part of CiviKey. 
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
using CK.Core;
using System.ComponentModel;

namespace Yodii.Model
{
    /// <summary>
    /// Yodii engine base interface for <see cref="IYodiiEngineExternal"/> and the <see cref="IYodiiEngine"/> avialable to plugins.
    /// It exposes all relevant information to the external or internal world thanks to its <see cref="LiveInfo"/>.
    /// </summary>
    public interface IYodiiEngineBase
    {
        /// <summary>
        /// Gets the configuration: gives access to static configuration that will 
        /// necessarily be always satisfied.
        /// </summary>
        IConfigurationManager Configuration { get; }
        
        /// <summary>
        /// Live information about the running services and plugins, when the engine is started.
        /// </summary>
        ILiveInfo LiveInfo { get; }

        /// <summary>
        /// Attempts to start a service or a plugin. 
        /// </summary>
        /// <param name="pluginOrService">The plugin or service live object to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// The <see cref="ILiveYodiiItem.Capability">pluginOrService.Capability</see> property <see cref="ILiveRunCapability.CanStart"/> 
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </para>
        /// or
        /// <para>
        /// This engine is not running.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter <paramref name="pluginOrService"/> is null.
        /// </exception>
        /// <returns>Result detailing whether the service or plugin was successfully started or not.</returns>
        IYodiiEngineResult StartItem( ILiveYodiiItem pluginOrService, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null );

        /// <summary>
        /// Attempts to stop this service or plugin.
        /// </summary>
        /// <param name="pluginOrService">The plugin or service live object to stop.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <returns>Result detailing whether the service or plugin was successfully stopped or not.</returns>
        /// <exception cref="InvalidOperationException">
        /// <para>
        /// The <see cref="ILiveYodiiItem.Capability">pluginOrService.Capability</see> property <see cref="ILiveRunCapability.CanStop"/> must be true.
        /// </para>
        /// or
        /// <para>
        /// This engine is not running.
        /// </para>
        /// </exception>
        /// <exception cref="ArgumentNullException">
        /// The parameter <paramref name="pluginOrService"/> is null.
        /// </exception>
        IYodiiEngineResult StopItem( ILiveYodiiItem pluginOrService, string callerKey = null );

        /// <summary>
        /// Attempts to start a plugin. 
        /// </summary>
        /// <param name="pluginFullName">Name of the plugin to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStart"/>  property  
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The plugin must exist.</exception>
        /// <returns>Result detailing whether the plugin was successfully started or not.</returns>
        IYodiiEngineResult StartPlugin( string pluginFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null );

        /// <summary>
        /// Attempts to stop a plugin. 
        /// </summary>
        /// <param name="pluginFullName">Name of the plugin to stop.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStop"/>' property must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The plugin must exist.</exception>
        /// <returns>Result detailing whether the service or plugin was successfully stopped or not.</returns>
        IYodiiEngineResult StopPlugin( string pluginFullName, string callerKey = null );

        /// <summary>
        /// Attempts to start a service. 
        /// </summary>
        /// <param name="serviceFullName">Name of the service to start.</param>
        /// <param name="impact">Startup impact on references.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStart"/>  property  
        /// or <see cref="ILiveRunCapability.CanStartWith"/> method must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The service must exist.</exception>
        /// <returns>Result detailing whether the service was successfully started or not.</returns>
        IYodiiEngineResult StartService( string serviceFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null );

        /// <summary>
        /// Attempts to stop a service. 
        /// </summary>
        /// <param name="serviceFullName">Name of the service to stop.</param>
        /// <param name="callerKey">Identifier of the caller. Can be any string. It is the plugin full name when called from a plugin.</param>
        /// <exception cref="InvalidOperationException">
        /// The <see cref="ILiveYodiiItem.Capability"/>.<see cref="ILiveRunCapability.CanStop"/>' property must be true.
        /// </exception>
        /// <exception cref="ArgumentException">The service must exist.</exception>
        /// <returns>Result detailing whether the service was successfully stopped or not.</returns>
        IYodiiEngineResult StopService( string serviceFullName, string callerKey = null );

    }
}
