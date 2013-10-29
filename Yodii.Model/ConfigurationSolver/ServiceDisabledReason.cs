﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model.CoreModel;

namespace Yodii.Model.ConfigurationSolver
{
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
        MultipleSpecializationsMustExistByConfig,

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
        AnotherSpecializationMustExist,

        /// <summary>
        /// Sets by ServiceData.OnSpecializationDisabled (that is called by ServiceData.SetDisabled).
        /// </summary>
        MustExistSpecializationIsDisabled,

        /// <summary>
        /// Sets by ServiceData.OnAllPluginsAdded method.
        /// </summary>
        NoPlugin,
        
        /// <summary>
        /// Sets by ServiceData.OnAllPluginsAdded method and ServiceData.OnPluginDisabled.
        /// </summary>
        AllPluginsAreDisabled,

        /// <summary>
        /// The service is not a dynamic service (it does not extend <see cref="IDynamicService"/>) and can not be 
        /// found in the Service provider. 
        /// </summary>
        ExternalServiceUnavailable
    }

}