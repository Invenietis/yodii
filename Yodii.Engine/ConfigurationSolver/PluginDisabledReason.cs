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
        PropagationFailed,
        AnotherRunningPluginExistsInFamilyByConfig,
        ServiceSpecializationRunning,
        RunningReferenceIsDisabled,
        RecommendedReferenceIsDisabled,
        OptionalReferenceIsDisabled,
        ByRunningReference,
        ByRunnableReference,
        ByRunnableRecommendedReference,
        ByOptionalRecommendedReference,
        ByOptionalReference,
        InvalidStructureLoop,
    }
}
