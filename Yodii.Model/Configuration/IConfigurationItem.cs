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
        /// Attempts to change the required ConfigurationStatus of this item.
        /// </summary>
        /// <param name="newStatus">New status for this item.</param>
        /// <param name="statusReason">Optional reason for this status.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult SetStatus( ConfigurationStatus newStatus, string statusReason = "" );
        
        /// <summary>
        /// Attempts to change the required StartDependencyImpact of this item.
        /// </summary>
        /// <param name="newImpact">New impact for this item.</param>
        /// <param name="impactReason">Optional reason for this impact.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult SetImpact( StartDependencyImpact newImpact, string impactReason = "" );

        /// <summary>
        /// Required configuration status for this item.
        /// </summary>
        ConfigurationStatus Status { get; }

        /// <summary>
        /// Required configuration impact for this item.
        /// </summary>
        StartDependencyImpact Impact { get; }

        /// <summary>
        /// Description of last status change reason.
        /// </summary>
        string StatusReason { get; set; }

        /// <summary>
        /// Description of last impact change reason.
        /// </summary>
        string ImpactReason { get; set; }
    }
}
