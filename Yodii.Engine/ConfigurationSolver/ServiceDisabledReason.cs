#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\ServiceDisabledReason.cs) is part of CiviKey. 
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
    /// Reasons for which a service was disabled.
    /// </summary>
    enum ServiceDisabledReason
    {
        /// <summary>
        /// The service is not disabled.
        /// </summary>
        None,

        /// <summary>
        /// Initialized by ServiceData constructor.
        /// </summary>
        Config,

        /// <summary>
        /// Initialized by ServiceData constructor.
        /// </summary>
        ServiceInfoHasError,

        /// <summary>
        /// Initialized by ServiceData constructor.
        /// </summary>
        GeneralizationIsDisabledByConfig,

        /// <summary>
        /// Sets by ServiceData.SetRunningRequirement method.
        /// </summary>
        RequirementPropagationToSinglePluginFailed,

        /// <summary>
        /// Sets by ServiceData.RetrieveOrUpdateTheCommonServiceReferences method.
        /// </summary>
        RequirementPropagationToCommonPluginReferencesFailed,

        /// <summary>
        /// Sets by ServiceData.GetMustExistService.
        /// </summary>
        MultipleSpecializationsRunningByConfig,

        /// <summary>
        /// Sets by ServiceData.GetMustExistService.
        /// </summary>
        AnotherSpecializationMustExistByConfig,

        /// <summary>
        /// Sets by ServiceData.SetDisabled.
        /// </summary>
        GeneralizationIsDisabled,

        /// <summary>
        /// Sets by ServiceData.SetRunningRequirement method.
        /// </summary>
        AnotherSpecializationMustRun,

        /// <summary>
        /// Sets by ServiceData.OnAllPluginsAdded method.
        /// </summary>
        NoPlugin,

        /// <summary>
        /// Sets by ServiceData.OnAllPluginsAdded method and ServiceData.OnPluginDisabled.
        /// </summary>
        AllPluginsAreDisabled,

        /// <summary>
        /// The service is not a dynamic service (it does not extend IDynamicService) and can not be 
        /// found in the Service provider. 
        /// </summary>
        ExternalServiceUnavailable,

        /// <summary>
        /// 
        /// </summary>
        ExcludingServiceIsBlocking,
        AnotherServiceIsRunningByConfig,
        PluginRunningAbove,
        SiblingSpecializationRunning,
        PluginRunningElsewhere,
        StopppedByPropagation,
        AnotherServiceRunningInFamily,
        PropagationFailed,
        PropagationToSinglePluginFailed,
        PropagationToCommonPluginReferencesFailed,
        RunningServiceDisabled,
        PropagationToSingleSpecializationFailed,
        AtLeastTwoSpecializationsMustRun
    }

}
