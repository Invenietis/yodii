using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Transition context for <see cref="IYodiiPlugin.PreStart"/>.
    /// </summary>
    public interface IPreStartContext
    {
        /// <summary>
        /// Cancels the start with an optional exception and/or message.
        /// </summary>
        /// <param name="message">Reason to not start.</param>
        /// <param name="ex">Optional exception that occurred.</param>
        void Cancel( string message = null, Exception ex = null );

        /// <summary>
        /// Gets a shared storage that is available during the whole transition.
        /// Used to exchange any kind of information during a transition: this typically 
        /// holds data that enables plugin hot swapping.
        /// </summary>
        IDictionary<object, object> SharedMemory { get; }

        /// <summary>
        /// Gets or sets the action that will be executed if any other PreStart fails.
        /// Note that this rollback action will not be called for the plugin that called <see cref="Cancel"/>.
        /// Defaults to <see cref="IYodiiPlugin.Stop"/>.
        /// </summary>
        Action<IStopContext> RollbackAction { get; set; }

        /// <summary>
        /// Gets the most specialized service that the starting plugin implements
        /// and is also implemented by a plugin that is stopping. 
        /// </summary>
        IYodiiService PreviousPluginCommonService { get; }

        /// <summary>
        /// Gets the previous plugin that also implements <see cref="PreviousPluginCommonService"/> that 
        /// has just been stopped: the starting plugin may set <see cref="HotSwapping"/> to true
        /// to silently replace it.
        /// </summary>
        IYodiiPlugin PreviousPlugin { get; }

        /// <summary>
        /// Gets or sets whether the plugin silently replaces the <see cref="PreviousPlugin"/>. 
        /// PreviousPlugin MUST not be null otherwise an <see cref="InvalidOperationException"/> is thrown.
        /// Defaults to false. When set to true, observers of the <see cref="PreviousPluginCommonService"/>
        /// (and its generalizaions if any) will not receive any events.
        /// </summary>
        bool HotSwapping { get; set; }

    }
}
