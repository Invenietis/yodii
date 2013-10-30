using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Describes how a service or a plugin is required. 
    /// A requirement is a gradation between <see cref="Optional"/> and <see cref="MustExistAndRun"/>.
    /// </summary>
    [Flags]
    public enum RunningRequirement
    {
        /// <summary>
        /// The service or plugin is optional: it can be unavailable.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// If it is available the service or plugin should be started.
        /// </summary>
        OptionalTryStart = 1,

        /// <summary>
        /// The service or plugin must be available (ready to run but it can be stopped if nothing else want to start it).
        /// It is guaranteed to be runnable.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// The service or plugin must be available and intially started. It can be stopped later.
        /// </summary>
        RunnableTryStart = 5,

        /// <summary>
        /// The service or plugin must be available and must be running.
        /// </summary>
        Running = 2 + 4
    }
}
