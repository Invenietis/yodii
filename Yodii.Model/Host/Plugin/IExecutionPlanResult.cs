using System;

namespace Yodii.Model
{
    /// <summary>
    /// Defines the return of the <see cref="IPluginHost.Execute"/> method.
    /// </summary>
    public interface IExecutionPlanResult
    {
        /// <summary>
        /// Kind of error.
        /// </summary>
        ExecutionPlanResultStatus Status { get; }

        /// <summary>
        /// The plugin that raised the error.
        /// </summary>
        IPluginInfo Culprit { get; }

        /// <summary>
        /// Detailed error information specific to the <see cref="IPlugin.Setup"/> phasis.
        /// </summary>
        PluginSetupInfo SetupInfo { get; }

        /// <summary>
        /// Gets the exception if it exists (note that a <see cref="IPlugin.Setup"/> may not throw exception but simply 
        /// returns false).
        /// </summary>
        Exception Error { get; }
    }
}
