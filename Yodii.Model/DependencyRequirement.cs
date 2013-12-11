using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Describes how a plugin requires a service. 
    /// A requirement is a gradation between <see cref="Optional"/> and <see cref="Running"/>.
    /// </summary>
    [Flags]
    public enum DependencyRequirement
    {
        /// <summary>
        /// The service is optional: it can be unavailable.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// If the service is available, it is better if it is started (it is a "recommended" service).
        /// </summary>
        OptionalTryStart = 1,

        /// <summary>
        /// The service must be available (ready to run but it can be stopped if nothing else want to start it).
        /// It is guaranteed to be runnable.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// The service must be available and it is better if it is started (it is a "recommended" service). 
        /// It can always be stopped at any time.
        /// </summary>
        RunnableTryStart = 5,

        /// <summary>
        /// The service must be running.
        /// </summary>
        Running = 2 + 4
    }
}
