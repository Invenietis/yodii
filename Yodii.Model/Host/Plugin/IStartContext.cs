using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Transition context for <see cref="IYodiiPlugin.Start"/>.
    /// </summary>
    public interface IStartContext
    {
        /// <summary>
        /// Gets whether this stop is from a cancelled <see cref="IPlugin.PreStop"/> rather
        /// than a successful <see cref="IPlugin.PreStart"/>.
        /// </summary>
        bool CancellingPreStop { get; }

        /// <summary>
        /// Gets a shared storage that is available during the whole transition.
        /// Used to exchange any kind of information during a transition: this typically 
        /// holds data that enables plugin hot swapping.
        /// </summary>
        IDictionary<object, object> SharedMemory { get; }

        /// <summary>
        /// Gets whether the plugin silently replaces the <see cref="IPreStartContext.PreviousPlugin"/> if any.
        /// </summary>
        bool HotSwapping { get; }
    }
}
