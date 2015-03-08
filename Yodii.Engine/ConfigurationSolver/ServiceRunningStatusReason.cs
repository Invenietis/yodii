#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\ServiceRunningStatusReason.cs) is part of CiviKey. 
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
    /// <summary>
    /// Description of the reason behind a running/stopped service.
    /// </summary>
    enum ServiceRunningStatusReason
    {
        /// <summary>
        /// No reason.
        /// </summary>
        None = 0,

        /// <summary>
        /// Configuration required this service to start.
        /// </summary>
        StartedByConfig,

        /// <summary>
        /// Configuration disabled this service.
        /// </summary>
        StoppedByConfig,

        /// <summary>
        /// A generalization service required this service to stop.
        /// </summary>
        StoppedByGeneralization,

        /// <summary>
        /// Service stopped by command.
        /// </summary>
        StoppedByCommand,

        /// <summary>
        /// A plugin implementing this service was stopped.
        /// </summary>
        StoppedByPluginStopped,

        /// <summary>
        /// A specialized service required this service to stop.
        /// </summary>
        StartedBySpecialization,

        /// <summary>
        /// Service stopped because one if its service siblings started.
        /// </summary>
        StoppedBySiblingRunningService,

        /// <summary>
        /// Service started by command.
        /// </summary>
        StartedByCommand,

        /// <summary>
        /// Service started by one if its plugins.
        /// </summary>
        StartedByPlugin,

        /// <summary>
        /// Service started by a plugin's Optional reference to this service.
        /// </summary>
        StartedByOptionalReference,
        /// <summary>
        /// Service started by a plugin's OptionalRecommended reference to this service.
        /// </summary>
        StartedByOptionalRecommendedReference,
        /// <summary>
        /// Service started by a plugin's Runnable reference to this service.
        /// </summary>
        StartedByRunnableReference,
        /// <summary>
        /// Service started by a plugin's RunnableRecommended reference to this service.
        /// </summary>
        StartedByRunnableRecommendedReference,
        /// <summary>
        /// Service started by a plugin's Running reference to this service.
        /// </summary>
        StartedByRunningReference,

        /// <summary>
        /// Service stopped by a plugin's Optional reference.
        /// </summary>
        StoppedByOptionalReference,
        /// <summary>
        /// Service stopped by a plugin's OptionalRecommended reference.
        /// </summary>
        StoppedByOptionalRecommendedReference,
        /// <summary>
        /// Service stopped by a plugin's Runnable reference.
        /// </summary>
        StoppedByRunnableReference,
        /// <summary>
        /// Service stopped by a plugin's RunnableRecommended reference.
        /// </summary>
        StoppedByRunnableRecommendedReference,

        /// <summary>
        /// Service stopped when reaching end of resolution.
        /// </summary>
        StoppedByFinalDecision,
        StoppedByPropagation,
        StartedByPropagation,
        StoppedByRunningReference,
    }
}
