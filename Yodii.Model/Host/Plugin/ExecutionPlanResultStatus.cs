using System;

namespace Yodii.Model
{
    /// <summary>
    /// Qualifies the type of error during plugin management.
    /// </summary>
    public enum ExecutionPlanResultStatus
    {
        /// <summary>
        /// No error.
        /// </summary>
        Success = 0,

        /// <summary>
        /// An error occured while loading (activating) the plugin.
        /// </summary>
        LoadError = 1,

        /// <summary>
        /// An error occured during the call to <see cref="IPlugin.Setup"/>.
        /// </summary>
        SetupError = 2,

        /// <summary>
        /// An error occured during the call to <see cref="IPlugin.Start"/>.
        /// </summary>
        StartError = 3
    }
}
