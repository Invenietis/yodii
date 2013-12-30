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
        /// Service started by a plugin's OptionalTryStart reference to this service.
        /// </summary>
        StartedByOptionalTryStartReference,
        /// <summary>
        /// Service started by a plugin's Runnable reference to this service.
        /// </summary>
        StartedByRunnableReference,
        /// <summary>
        /// Service started by a plugin's RunnableTryStart reference to this service.
        /// </summary>
        StartedByRunnableTryStartReference,
        /// <summary>
        /// Service started by a plugin's Running reference to this service.
        /// </summary>
        StartedByRunningReference,

        /// <summary>
        /// Service stopped by a plugin's Optional reference.
        /// </summary>
        StoppedByOptionalReference,
        /// <summary>
        /// Service stopped by a plugin's OptionalTryStart reference.
        /// </summary>
        StoppedByOptionalTryStartReference,
        /// <summary>
        /// Service stopped by a plugin's Runnable reference.
        /// </summary>
        StoppedByRunnableReference,
        /// <summary>
        /// Service stopped by a plugin's RunnableTryStart reference.
        /// </summary>
        StoppedByRunnableTryStartReference,

        /// <summary>
        /// Service stopped when reaching end of resolution.
        /// </summary>
        StoppedByFinalDecision,
        StoppedByPropagation,
        StartedByPropagation,
        StoppedByRunningReference,
    }
}
