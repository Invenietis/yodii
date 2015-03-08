using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Describes the error related to Start/Stop methods like <see cref="IYodiiEngineBase.StartItem"/> or <see cref="IYodiiEngineBase.StopItem"/>.
    /// </summary>
    public interface ICommandFailureResult
    {
        /// <summary>
        /// Gets the plugin or service name that failed to start or stop.
        /// </summary>
        string PluginOrServiceFullName { get; }

        /// <summary>
        /// Gets whether the command is non applicable like a Stop on a necessarily running item.
        /// To avoid this error, before calling Start or Stop <see cref="ILiveYodiiItem.Capability"/> 
        /// (see <see cref="ILiveRunCapability"/>) must be tested before.
        /// </summary>
        bool UnapplicableCommand { get; }
        
        /// <summary>
        /// Gets whether <see cref="PluginOrServiceFullName"/> has not been found.
        /// </summary>
        bool UnexistingItem { get; }
    }
}
