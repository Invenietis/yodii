using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Updater
{
    /// <summary>
    /// The progress of an updater task.
    /// </summary>
    public struct UpdaterTaskProgress : IUpdaterTaskProgress
    {
        readonly double _progress;
        readonly string _statusText;

        /// <summary>
        /// Initializes a new instance of the <see cref="UpdaterTaskProgress"/> struct.
        /// </summary>
        /// <param name="statusText">The status text of the task.</param>
        /// <param name="progress">The progress of the task, bounded between 0.0 and 1.0.</param>
        public UpdaterTaskProgress( string statusText, double progress )
        {
            if( progress < 0.0 || progress > 1.0 ) { throw new ArgumentException( "progress must be bounded between 0.0 inclusive and 1.0 inclusive.", "progress" ); }

            _progress = progress;
            _statusText = statusText ?? String.Empty;
        }

        /// <summary>
        /// Gets the status text of the task.
        /// </summary>
        /// <value>
        /// The status text of the task.
        /// </value>
        public string StatusText
        {
            get { return _statusText; }
        }

        /// <summary>
        /// Gets the progress of the task, bounded between 0.0 and 1.0.
        /// </summary>
        /// <value>
        /// The progress of the task, bounded between 0.0 and 1.0.
        /// </value>
        public double Progress
        {
            get { return _progress; }
        }
    }
}
