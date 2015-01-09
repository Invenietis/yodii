#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\PluginRunningStatusReason.cs) is part of CiviKey. 
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
    /// Description of the reason behind a running/stopped plugin.
    /// </summary>
    enum PluginRunningStatusReason
    {
        /// <summary>
        /// No reason.
        /// </summary>
        None = 0,

        /// <summary>
        /// Configuration required this plugin to be started.
        /// </summary>
        StartedByConfig,

        /// <summary>
        /// Configuration required this plugin to be stopped.
        /// </summary>
        StoppedByConfig,

        /// <summary>
        /// A running implemented service required this plugin to start.
        /// </summary>
        StartedByRunningService,

        /// <summary>
        /// A stopping implemented service required this plugin to stop.
        /// </summary>
        StoppedByStoppedService,

        /// <summary>
        /// Plugin started by command.
        /// </summary>
        StartedByCommand,

        /// <summary>
        /// Plugin stopped by command.
        /// </summary>
        StoppedByCommand,

        /// <summary>
        /// Plugin stopped because one of its siblings was started.
        /// </summary>
        StoppedByRunningSibling,

        /// <summary>
        /// Plugin stopped because one if the services it references was stopped.
        /// </summary>
        StoppedByStoppedReference,

        /// <summary>
        /// Plugin stopped during the end of resolution.
        /// </summary>
        StoppedByFinalDecision,
        StartedByFinalDecision,
        StoppedByRunningReference,
        StoppedByRunnableRecommendedReference,
        StoppedByRunnableReference,
        StoppedByOptionalRecommendedReference,
        StoppedByOptionalReference,
        StoppedByServiceCommandImpact
    }
}
