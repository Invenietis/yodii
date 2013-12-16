using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Impact of a plugin start on its dependencies.
    /// </summary>
    /// <remarks>
    /// Defines whether the plugin starting will also attempt to start everything it has references to,
    /// or only the minimum required services.
    /// </remarks>
    public enum StartDependencyImpact
    {
        /// <summary>
        /// Impact is not determined. Ultimately defaults to <see cref="Minimal"/>.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Attempts to stop all dependencies: <see cref="DependencyRequirement.Optional"/>
        /// and <see cref="DependencyRequirement.Runnable"/>, but also the "recommended" ones (<see cref="DependencyRequirement.OptionalTryStart"/>
        /// and <see cref="DependencyRequirement.RunnableTryStart"/>).
        /// </summary>
        FullStop = 1,

        /// <summary>
        /// Attempts to stop "not recommended" dependencies (<see cref="DependencyRequirement.Optional"/>
        /// and <see cref="DependencyRequirement.Runnable"/>).
        /// </summary>
        StopOptionalAndRunnable = 2,

        /// <summary>
        /// Do not try to start or stops dependencies that are not absolutely required (<see cref="DependencyRequirement.Running"/>).
        /// This is the default.
        /// </summary>
        Minimal = 3,

        /// <summary>
        /// Attempts to start "recommended" dependencies: the ones that are <see cref="DependencyRequirement.OptionalTryStart"/>
        /// and <see cref="DependencyRequirement.RunnableTryStart"/>.
        /// </summary>
        StartRecommended = 4,

        /// <summary>
        /// Attempts to start all dependencies: <see cref="DependencyRequirement.OptionalTryStart"/>, 
        /// <see cref="DependencyRequirement.RunnableTryStart"/>, but also the "not recommended" ones (<see cref="DependencyRequirement.Optional"/> 
        /// and <see cref="DependencyRequirement.Runnable"/>).
        /// </summary>
        FullStart = 5

    }
}
