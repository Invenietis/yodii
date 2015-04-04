using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Updater
{
    /// <summary>
    /// The result of an updater task.
    /// </summary>
    public interface IUpdaterTaskResult
    {
        /// <summary>
        /// Gets the final status of this task.
        /// </summary>
        /// <value>
        /// The final status of this task.
        /// </value>
        UpdaterTaskStatus Status { get; }

        /// <summary>
        /// Gets the exception that occured during task processing.
        /// </summary>
        /// <value>
        /// The exception that occured during task processing, is <see cref="Status"/> proerty is equal to <see cref="UpdaterTaskStatus.Error"/>.
        /// </value>
        Exception Exception { get; }
    }
}
