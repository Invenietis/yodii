using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Updater
{
    /// <summary>
    /// Updater task status.
    /// </summary>
    public enum UpdaterTaskStatus
    {
        /// <summary>
        /// Task has started, but has not been completed yet.
        /// </summary>
        Ongoing,

        /// <summary>
        /// The task has been successfully completed.
        /// </summary>
        Complete,

        /// <summary>
        /// The task has been canceled by the user.
        /// </summary>
        Canceled,

        /// <summary>
        /// An error has occured during task execution.
        /// The <see cref="IUpdaterTaskResult.Exception"/> property contains the exception that was caught.
        /// </summary>
        Error
    }
}
