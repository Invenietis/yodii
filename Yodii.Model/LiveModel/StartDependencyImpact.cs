using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Impact of a plugin or service start on its dependencies.
    /// </summary>
    /// <remarks>
    /// Defines whether the plugin starting will also attempt to start everything it has references to,
    /// or only the minimum required services.
    /// </remarks>
    [Flags]
    public enum StartDependencyImpact
    {
        /// <summary>
        /// Impact is not determined. Ultimately defaults to <see cref="Minimal"/>.
        /// </summary>
        Unknown = 0,
   
        /// <summary>
        /// Do not try to start nor stop dependencies that are not absolutely required (<see cref="DependencyRequirement.Running"/>).
        /// This is the default.
        /// </summary>
        Minimal = 1,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.RunnableRecommended"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartRunnableRecommended"/>.
        /// </summary>
        IsStartRunnableRecommended = 2,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.OptionalRecommended"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartOptionalRecommended"/>.
        /// </summary>
        IsStartOptionalRecommended = 4,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.Runnable"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartRunnableOnly"/>.
        /// </summary>
        IsStartRunnableOnly = 8,

        /// <summary>
        /// Starts <see cref="DependencyRequirement.Optional"/> dependencies.
        /// When this bit is set, it takes precedence over <see cref="IsTryStartOptionalOnly"/>.
        /// </summary>
        IsStartOptionalOnly = 16,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.RunnableRecommended"/> dependencies.
        /// </summary>
        IsTryStartRunnableRecommended = 32,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.OptionalRecommended"/> dependencies.
        /// </summary>
        IsTryStartOptionalRecommended = 64,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.Runnable"/> dependencies.
        /// </summary>
        IsTryStartRunnableOnly = 128,

        /// <summary>
        /// Attempts to start <see cref="DependencyRequirement.Optional"/> dependencies.
        /// </summary>
        IsTryStartOptionalOnly = 256,

        /// <summary>
        /// Starts recommended dependencies: the ones that are <see cref="DependencyRequirement.OptionalRecommended"/>
        /// and <see cref="DependencyRequirement.RunnableRecommended"/>.
        /// </summary>
        StartRecommended = IsStartOptionalRecommended | IsStartRunnableRecommended,

        /// <summary>
        /// Attempts to start recommended dependencies: the ones that are <see cref="DependencyRequirement.OptionalRecommended"/>
        /// and <see cref="DependencyRequirement.RunnableRecommended"/>.
        /// </summary>
        TryStartRecommended = IsTryStartOptionalRecommended | IsTryStartRunnableRecommended,

        /// <summary>
        /// Starts all dependencies: <see cref="DependencyRequirement.OptionalRecommended"/>, <see cref="DependencyRequirement.RunnableRecommended"/>, 
        /// but also the "not recommended" ones (<see cref="DependencyRequirement.Optional"/> and <see cref="DependencyRequirement.Runnable"/>).
        /// </summary>
        FullStart = StartRecommended | IsStartRunnableOnly | IsStartOptionalOnly, 

        /// <summary>
        /// Attempts to start all dependencies: <see cref="DependencyRequirement.OptionalRecommended"/>, 
        /// <see cref="DependencyRequirement.RunnableRecommended"/>, but also the "not recommended" ones (<see cref="DependencyRequirement.Optional"/> 
        /// and <see cref="DependencyRequirement.Runnable"/>).
        /// </summary>
        TryFullStart = TryStartRecommended | IsTryStartRunnableOnly | IsTryStartOptionalOnly 
    }
}
