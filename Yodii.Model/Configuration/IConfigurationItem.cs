using System;
using System.ComponentModel;
namespace Yodii.Model
{
    /// <summary>
    /// Configuration item.
    /// </summary>
    public interface IConfigurationItem : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the parent configuration layer.
        /// </summary>
        IConfigurationLayer Layer { get; }

        /// <summary>
        /// Service or plugin identifier this configuration applies to.
        /// </summary>
        string ServiceOrPluginFullName { get; }

        /// <summary>
        /// Attempts to change the required <see cref="Status"/> of this item.
        /// </summary>
        /// <param name="newStatus">New status for this item.</param>
        /// <param name="newDescription">Optional description to set. When null, current description is preserved.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult Set( ConfigurationStatus newStatus, string newDescription = null );
        
        /// <summary>
        /// Attempts to change the required <see cref="Impact"/> of this item.
        /// </summary>
        /// <param name="newImpact">New impact for this item.</param>
        /// <param name="newDescription">Optional description to set. When null, current description is preserved.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult Set( StartDependencyImpact newImpact, string newDescription = null );

        /// <summary>
        /// Attempts to change the <see cref="Status"/> and the <see cref="Impact"/> and optionaly sets the description.
        /// </summary>
        /// <param name="newStatus">New status for this item.</param>
        /// <param name="newImpact">New impact for this item.</param>
        /// <param name="newDescription">Optional description to set. When null, current description is preserved.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult Set( ConfigurationStatus newStatus, StartDependencyImpact newImpact, string newDescription = null );

        /// <summary>
        /// Gets the required configuration status for this item.
        /// </summary>
        ConfigurationStatus Status { get; }

        /// <summary>
        /// Gets the required configuration impact for this item.
        /// </summary>
        StartDependencyImpact Impact { get; }

        /// <summary>
        /// Gets or sets an optional description for this configuration.
        /// This is null when <see cref="Layer"/> is null (this item does no more belong to its layer).
        /// </summary>
        string Description { get; set; }
    }
}
