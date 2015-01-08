#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Solver\IStaticSolvedYodiiItem.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Generalizes static solved plugin or service information.
    /// </summary>
    public interface IStaticSolvedYodiiItem
    {
        /// <summary>
        /// Gets the <see cref="IPluginInfo.PluginFullName"/> or the <see cref="IServiceInfo.ServiceFullName"/>.
        /// </summary>
        string FullName { get; }

        /// <summary>
        /// Gets a string that describes the reason for a disabled status.
        /// Null when this item is not disabled.
        /// </summary>
        string DisabledReason { get; }

        /// <summary>
        /// Status as set by initial configuration.
        /// </summary>
        ConfigurationStatus ConfigOriginalStatus { get; }

        /// <summary>
        /// Dependency impact as set by initial configuration.
        /// </summary>
        StartDependencyImpact ConfigOriginalImpact { get; }

        /// <summary>
        /// Final dependency impact. 
        /// <para>
        /// For a plugin it is the Service's one if this plugin implements a Service and 
        /// this <see cref="ConfigOriginalImpact"/> is <see cref="StartDependencyImpact.Unknown"/>.
        /// </para>
        /// <para>
        /// For a Service, it is the Generalization's one if this service specializes another one and 
        /// this <see cref="ConfigOriginalImpact"/> is <see cref="StartDependencyImpact.Unknown"/>.
        /// </para>
        /// </summary>
        StartDependencyImpact ConfigSolvedImpact { get; }

        /// <summary>
        /// Gets the solved configuration status, it is the wanted result: it can be <see cref="SolvedConfigurationStatus.Running"/>
        /// even if the <see cref="DisabledReason"/> is not null.
        /// Use <see cref="FinalConfigSolvedStatus"/> to get a status that integrates the fact that the item is disabled.
        /// </summary>
        SolvedConfigurationStatus WantedConfigSolvedStatus { get; }

        /// <summary>
        /// Gets the final configuration status, it is the result of the static resolution phase.
        /// </summary>
        SolvedConfigurationStatus FinalConfigSolvedStatus { get; }

        /// <summary>
        /// Gets whether this item blocks static resolution.
        /// </summary>
        bool IsBlocking { get; }
    }
}
