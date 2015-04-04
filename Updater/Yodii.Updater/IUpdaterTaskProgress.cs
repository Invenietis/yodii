using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yodii.Updater
{
    /// <summary>
    /// Updater task progress, reported through IProgress.
    /// </summary>
    public interface IUpdaterTaskProgress
    {
        /// <summary>
        /// Gets the progress of the task, bounded between 0.0 and 1.0.
        /// </summary>
        /// <value>
        /// The progress of the task, bounded between 0.0 and 1.0.
        /// </value>
        double Progress { get; }

        /// <summary>
        /// Gets the status text of the task.
        /// </summary>
        /// <value>
        /// The status text of the task.
        /// </value>
        string StatusText { get; }
    }
}
