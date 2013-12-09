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
        /// Parent configuration layer.
        /// </summary>
        IConfigurationLayer Layer { get; }

        /// <summary>
        /// Service or plugin ID this configuration applies to.
        /// </summary>
        string ServiceOrPluginId { get; }

        /// <summary>
        /// Attempts to change the required ConfigurationStatus of this item.
        /// </summary>
        /// <param name="newStatus">Status to change to</param>
        /// <param name="statusReason">Description of the change</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult SetStatus( ConfigurationStatus newStatus, string statusReason = "" );

        /// <summary>
        /// Required configuration status for this item.
        /// </summary>
        ConfigurationStatus Status { get; }

        /// <summary>
        /// Description of last status change.
        /// </summary>
        string StatusReason { get; set; }
    }
}
