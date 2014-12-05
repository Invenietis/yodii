using System;
using System.Collections.Generic;

namespace Yodii.Model
{
    /// <summary>
    /// This interface defines the minimal properties and behavior of a plugin.
    /// It implements a two-phases transition: plugin that should stop or start
    /// can accept or reject the transition thanks to <see cref="PresStop"/> and <see cref="PreStart"/>.
    /// If all of them aggreed, then <see cref="Stop"/> and <see cref="Start"/> are called.
    /// </summary>
    public interface IYodiiPlugin
    {
        /// <summary>
        /// Called before the actual <see cref="Stop"/> method.
        /// Implementations must validate that this plugin can be stoppped: if not, the transition must 
        /// be canceled by calling <see cref="IPreStopContext.Cancel"/>.
        /// </summary>
        /// <param name="c">The context to use.</param>
        void PreStop( IPreStopContext c );

        /// <summary>
        /// Called before the actual <see cref="Start"/> method.
        /// Implementations must validate that the start is possible and, if unable 
        /// to start, cancels it by calling <see cref="IPreStartContext.Cancel"/> .
        /// </summary>
        /// <param name="c">The context to use.</param>
        void PreStart( IPreStartContext c );

        /// <summary>
        /// Called after successful calls to all <see cref="PreStop"/> and <see cref="PreStart"/>.
        /// This may also be called to cancel a previous call to <see cref="PreStart"/> if another
        /// plugin rejected the transition.
        /// </summary>
        /// <param name="c">The context to use.</param>
        void Stop( IStopContext c );

        /// <summary>
        /// Called after successful calls to all <see cref="PreStop"/> and <see cref="PreStart"/>.
        /// This may also be called to cancel a previous call to <see cref="PreStop"/> if another
        /// plugin rejected the transition.
        /// </summary>
        /// <param name="c">The context to use.</param>
        void Start( IStartContext c );
    }
}
