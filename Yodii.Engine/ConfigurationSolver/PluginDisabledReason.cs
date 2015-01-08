#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\PluginDisabledReason.cs) is part of CiviKey. 
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
    /// Reasons for which a plugin was disabled.
    /// </summary>
    enum PluginDisabledReason
    {
        /// <summary>
        /// The plugin is not disabled.
        /// </summary>
        None,

        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        Config,

        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        PluginInfoHasError,

        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        ServiceIsDisabled,

        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        RunnableReferenceServiceIsOnError,

        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        ServiceSpecializationMustRun,

        /// <summary>
        /// Sets by PluginData constructor or later by ServiceData.SetDisabled.
        /// </summary>
        RunnableReferenceIsDisabled,

        /// <summary>
        /// Set by ServiceRootData.SetMustExistPluginByConfig.
        /// </summary>
        AnotherPluginAlreadyExistsForTheSameService,

        /// <summary>
        /// Sets by PluginData.SetRunningRequirement.
        /// </summary>
        RequirementPropagationToReferenceFailed,
        AnotherRunningPluginExistsInFamily,
        ServiceCanNotBeRunning,
        SiblingRunningPlugin,
        AnotherRunningPluginExistsInFamilyByConfig,
        ServiceSpecializationRunning,
        ByRunningReference,
        ByRunnableReference,
        ByRunnableRecommendedReference,
        ByOptionalRecommendedReference,
        ByOptionalReference,
        InvalidStructureLoop,
    }
}
