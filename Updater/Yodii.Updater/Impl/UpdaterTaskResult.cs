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
    public class UpdaterTaskResult : IUpdaterTaskResult
    {
        readonly Exception _exception;
        readonly UpdaterTaskStatus _status;

        internal UpdaterTaskResult( UpdaterTaskStatus status )
        {
            _status = status;
        }

        internal UpdaterTaskResult( Exception e )
        {
            _status = UpdaterTaskStatus.Error;
            _exception = e;
        }

        /// <summary>
        /// Gets the final status of this task.
        /// </summary>
        /// <value>
        /// The final status of this task.
        /// </value>
        public UpdaterTaskStatus Status { get { return _status; } }

        /// <summary>
        /// Gets the exception that occured during task processing.
        /// </summary>
        /// <value>
        /// The exception that occured during task processing, is <see cref="Status"/> property is equal to <see cref="UpdaterTaskStatus.Error"/>.
        /// </value>
        public Exception Exception { get { return _exception; } }
    }
}
