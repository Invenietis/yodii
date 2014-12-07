﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using CK.Core;

namespace Yodii.Host
{
    /// <summary>
    /// Centralized management of log events.
    /// Even if these events are designed for <see cref="IServiceHost"/> behavior, 
    /// the <see cref="ExternalLog"/> and <see cref="ExternalLogError"/> methods 
    /// enable injection of external events into the pipe. 
    /// </summary>
    public interface ILogCenter
    {
        /// <summary>
        /// Fires when a <see cref="LogHostEventArgs"/> is beeing created.
        /// This event is "opened": it will be closed when the <see cref="EventCreated"/> fires.
        /// </summary>
        event EventHandler<LogEventArgs> EventCreating;

        /// <summary>
        /// Fires for each <see cref="LogHostEventArgs"/>.
        /// </summary>
        event EventHandler<LogEventArgs> EventCreated;

        /// <summary>
        /// Generates a <see cref="ILogExternalEntry"/> event log.
        /// </summary>
        /// <param name="message">Event message. Should be localized if possible.</param>
        /// <param name="extraData">Optional extra data associated to the event.</param>
        void ExternalLog( string message, object extraData );

        /// <summary>
        /// Generates a <see cref="ILogExternalErrorEntry"/> event log.
        /// </summary>
        /// <param name="e">The <see cref="Exception"/>. When null, a warning is added to the message.</param>
        /// <param name="optionalExplicitCulprit">
        /// Optional <see cref="MemberInfo"/> that designates a culprit. 
        /// It can be null: when not specified, the <see cref="Exception.TargetSite"/> is used.
        /// </param>
        /// <param name="message">Optional event message (localized if possible). Nullable.</param>
        /// <param name="extraData">Optional extra data associated to the event. Nullable.</param>
        void ExternalLogError( Exception e, MemberInfo optionalExplicitCulprit, string message, object extraData );

        /// <summary>
        /// Gets the list of errors that occured while there was no launched plugins to process them.
        /// </summary>
        IReadOnlyList<ILogErrorCaught> UntrackedErrors { get; }

    }
}
