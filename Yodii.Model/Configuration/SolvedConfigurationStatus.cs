using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Solved configuration status captures for any item (either a plugin or a service) the result of the static resolution.
    /// </summary>
    public enum SolvedConfigurationStatus
    {
        /// <summary>
        /// Item must be disabled, and cannot be run.
        /// </summary>
        Disabled = -1,

        /// <summary>
        /// Item can be started or stopped at any time.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// Item must run.
        /// </summary>
        Running = 6
    }
}
