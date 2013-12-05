using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Reasons for which a plugin was disabled.
    /// </summary>
    public enum PluginDisabledReason
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
        MustExistReferenceServiceIsOnError,

        /// <summary>
        /// Initialized by PluginData constructor.
        /// </summary>
        ServiceSpecializationMustExist,

        /// <summary>
        /// Sets by PluginData constructor or later by ServiceData.SetDisabled.
        /// </summary>
        MustExistReferenceIsDisabled,

        /// <summary>
        /// Set by ServiceRootData.SetMustExistPluginByConfig.
        /// </summary>
        AnotherPluginAlreadyExistsForTheSameService,

        /// <summary>
        /// Sets by PluginData.SetRunningRequirement.
        /// </summary>
        RequirementPropagationToReferenceFailed,
    }
}
