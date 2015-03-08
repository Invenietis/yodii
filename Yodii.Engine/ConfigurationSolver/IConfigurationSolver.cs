#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\IConfigurationSolver.cs) is part of CiviKey. 
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

namespace Yodii.Engine
{
    enum ConfigurationSolverStep
    {
        RegisterServices,
        RegisterPlugins,
        OnAllPluginsAdded,
        PropagatePluginStatus,
        InitializeFinalStartableStatus,
        BlockingDetection,
        DynamicResolution,
        StaticError,
        WaitingForDynamicResolution
    }

    /// <summary>
    /// Internal ConfigurationSolver interface.
    /// </summary>
    interface IConfigurationSolver
    {
        /// <summary>
        /// Gets the current step.
        /// </summary>
        ConfigurationSolverStep Step { get; }

        /// <summary>
        /// Finds a service by its name that must exist (otherwise an exception is thrown).
        /// </summary>
        /// <param name="serviceFullName">The service name.</param>
        /// <returns>The ServiceData.</returns>
        ServiceData FindExistingService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its name that must exist (otherwise an exception is thrown).
        /// </summary>
        /// <param name="pluginFullName">The plugin name.</param>
        /// <returns>The PluginData.</returns>
        PluginData FindExistingPlugin( string pluginFullName );

        /// <summary>
        /// Finds a service by its name. Returns null if it does not exist.
        /// </summary>
        /// <param name="serviceFullName">The service name.</param>
        /// <returns>Null if not found.</returns>
        ServiceData FindService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its name. Returns null if it does not exist.
        /// </summary>
        /// <param name="pluginFullName">The plugin name.</param>
        /// <returns>Null if not found.</returns>
        PluginData FindPlugin( string pluginFullName );
        
        /// <summary>
        /// Gets all the ServiceData ordered by their name.
        /// </summary>
        IEnumerable<ServiceData> AllServices { get; }
        
        /// <summary>
        /// Gets all the PluginData ordered by their name.
        /// </summary>
        IEnumerable<PluginData> AllPlugins { get; }
        
        /// <summary>
        /// Used during static resolution to stack propagation 
        /// instead of relying on recursion.
        /// </summary>
        /// <param name="s">The ServiceData for which status must be propagated.</param>
        void DeferPropagation( ServiceData s );

        /// <summary>
        /// Gets a set of ServiceData that enables the handling of co-dependent Service/Plugin for
        /// computing the Service.CanStartOrIsStarted property during dynamic resolution.
        /// This set is created by the first call to the DynamicResolution method.
        /// When Service.CanStartOrIsStarted needs to find at least one plugin that CanStartOrIsStarted
        /// it first checks it it already appears in this set:
        ///   - if yes: we return true since this means that we are already checking it and if everything else
        ///     is okay, the Service will actually be able to start.
        ///   - if no: the Service adds itself to the set, tries to find a plugin that can start, and removes 
        ///     itself from the set.
        /// </summary>
        ISet<ServiceData> RecursiveStartableServiceSet { get; }
    }

}
