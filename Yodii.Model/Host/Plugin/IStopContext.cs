using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Transition context for <see cref="IYodiiPlugin.Stop"/>.
    /// </summary>
    public interface IStopContext
    {
        /// <summary>
        /// Gets whether this stop is from a cancelled <see cref="IPlugin.PreStart"/> rather
        /// than a successful <see cref="IPlugin.PreStop"/>.
        /// </summary>
        bool CancellingPreStart { get; }

        /// <summary>
        /// Gets a shared storage that is available during the whole transition.
        /// Used to exchange any kind of information during a transition: this typically 
        /// holds data that enables plugin hot swapping.
        /// </summary>
        IDictionary<object, object> SharedMemory { get; }

        /// <summary>
        /// Gets whether the plugin is silently replaced by another one.
        /// This is always false when <see cref="CancellingPreStart"/> is true.
        /// </summary>
        bool HotSwapping { get; }

    }
}
