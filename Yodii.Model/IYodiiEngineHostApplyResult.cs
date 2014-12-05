using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace Yodii.Model
{

    /// <summary>
    /// Defines the returned result of <see cref="IYodiiEngineHost.Apply"/>.
    /// </summary>
    public interface IYodiiEngineHostApplyResult
    {
        /// <summary>
        /// Gets the errors if any. Never null (empty if none).
        /// </summary>
        IReadOnlyList<IPluginHostApplyCancellationInfo> CancellationInfo { get; }

        /// <summary>
        /// Gets the actions that must be triggered. Never null (empty if none).
        /// </summary>
        IReadOnlyList<Action<IYodiiEngine>> PostStartActions { get; }
    }

}
