using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Specifies the final configuration of a plugin or service.
    /// Takes into account the whole configuration and relationships between plugins and services.
    /// </summary>
    [Flags]
    public enum SolvedConfigurationStatus
    {
        /// <summary>
        /// The service or plugin is disabled.
        /// </summary>
        Disabled = -1,

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
        /// The service or plugin must be available and should be intially started. It can be stopped later.
        /// </summary>
        RunnableTryStart = 5,

        /// <summary>
        /// The service or plugin must be available and must be running.
        /// </summary>
        Running = 2 + 4
    }
}
