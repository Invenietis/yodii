using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Transition context for <see cref="IYodiiPlugin.PreStop"/>.
    /// </summary>
    public interface IPreStopContext
    {
        /// <summary>
        /// Cancels the stop with an optional exception and/or message.
        /// If for any reason a plugin can not or refuse to stop, this method must be called.
        /// </summary>
        /// <param name="message">Reason to reject the stop.</param>
        /// <param name="ex">Optional exception that occurred.</param>
        void Cancel( string message = null, Exception ex = null );

        /// <summary>
        /// Gets a shared storage that is available during the whole transition.
        /// Used to exchange any kind of information during a transition: this typically 
        /// holds data that enables plugin hot swapping.
        /// </summary>
        IDictionary<object, object> SharedMemory { get; }

        /// <summary>
        /// Gets or sets the action that will be executed if any other PreStop or PreStart fails.
        /// Note that this rollback action will not be called for the plugin that called <see cref="Cancel"/>.
        /// Defaults to <see cref="IYodiiPlugin.Start"/>.
        /// </summary>
        Action<IStartContext> RollbackAction { get; set; }
    }
}
