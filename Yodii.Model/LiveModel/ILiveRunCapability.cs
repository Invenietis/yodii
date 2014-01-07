using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Exposes <see cref="IDynamicSolvedYodiiItem"/> running capabilities: this shows the
    /// actual guaranties that the engine engine is able to offer at any time.
    /// </summary>
    public interface ILiveRunCapability : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets whether the plugin or service can be stopped.
        /// This is always true except if its <see cref="IDynamicSolvedYodiiItem.WantedConfigSolvedStatus"/> is <see cref="ConfigurationStatus.Running"/>.
        /// </summary>
        bool CanStop { get; }

        /// <summary>
        /// Gets whether the plugin or service can be started.
        /// As long as its <see cref="IDynamicSolvedYodiiItem.WantedConfigSolvedStatus"/> is not <see cref="ConfigurationStatus.Disabled"/>, it can run
        /// and will always be able to start with its <see cref="IDynamicSolvedYodiiItem.ConfigSolvedImpact"/>.
        /// </summary>
        bool CanStart { get; }

        /// <summary>
        /// Gets whether the plugin or service can be successfully started in <see cref="StartDependencyImpact.FullStart"/> mode.
        /// This requires all dependencies to be running (either the purely optional ones). 
        /// </summary>
        bool CanStartWithFullStart { get; }

        /// <summary>
        /// Gets whether the plugin or service can be successfully started in <see cref="StartDependencyImpact.StartRecommended"/> mode.
        /// This requires the recommended dependencies (<see cref="DependencyRequirement.OptionalTryStart"/> and <see cref="DependencyRequirement.RunnableTryStart"/>) to be running. 
        /// </summary>
        bool CanStartWithStartRecommended { get; }

        /// <summary>
        /// Gets whether the plugin or service can be successfully started in <see cref="StartDependencyImpact.StopOptionalAndRunnable"/> mode.
        /// Non recommended dependencies (<see cref="DependencyRequirement.Optional"/> and <see cref="DependencyRequirement.Runnable"/>) ares stopped,
        /// recommended ones may run or not.
        /// </summary>
        bool CanStartWithStopOptionalAndRunnable { get; }

        /// <summary>
        /// Gets whether the plugin or service can be successfully started in <see cref="StartDependencyImpact.FullStop"/> mode.
        /// This stops any dependencies that are not absolutely required (only the <see cref="DependencyRequirement.Running"/> are running, the other ones are stopped).
        /// </summary>
        bool CanStartWithFullStop { get; }

        /// <summary>
        /// Tests whether the plugin or service can be successfully started with the given impact.
        /// </summary>
        /// <param name="impact">The impact that must be satisfied.</param>
        /// <returns>True if the plugin or service can successfully start with the given impact.</returns>
        /// <remarks>
        /// When impact is <see cref="StartDependencyImpact.Unknown"/> or if <see cref="StartDependencyImpact.IsTryOnly"/> bit is set, this 
        /// corresponds to the <see cref="CanStart"/> property.
        /// </remarks>
        bool CanStartWith( StartDependencyImpact impact );

    }
}
