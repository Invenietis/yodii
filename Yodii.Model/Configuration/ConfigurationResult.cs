using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Result of a configuration change. Returned when adding/removing/setting <see cref="ConfigurationLayer"/> or <see cref="ConfigurationLayer"/>.
    /// Can be considered successful if it has no failure causes.
    /// <seealso cref="ConfigurationResult.IsSuccessful()"/>
    /// </summary>
    /// <remarks>Can be implicitly casted to bool (same as <see cref="ConfigurationResult.IsSuccessful()"/> ).</remarks>
    public class ConfigurationResult
    {
        readonly List<string> _causes;

        /// <summary>
        /// List of strings describing why this configuration change was refused.
        /// Will be empty if the change was accepted.
        /// </summary>
        public IReadOnlyList<string> FailureCauses
        {
            get
            {
                return _causes.AsReadOnly();
            }
        }

        /// <summary>
        /// Indicates whether this ConfigurationChange was successful or not.
        /// </summary>
        /// <returns>True if the change was successful, false otherwise.</returns>
        public bool IsSuccessful
        {
            get
            {
                // Considered successful if it has no failure cause
                return this.FailureCauses.Count == 0;
            }
        }

        /// <summary>
        /// Creates a new instance of a successful ConfigurationResult, which can be set as failed by adding failure causes.
        /// <seealso cref="<see cref="ConfigurationResult.AddFailureCause(string cause)"/>"/>
        /// </summary>
        internal ConfigurationResult()
        {
            _causes = new List<string>();
        }

        /// <summary>
        /// Creates a new instance of an unsuccessful ConfigurationResult, failed with the given descriptive cause.
        /// </summary>
        /// <param name="failureCause"></param>
        internal ConfigurationResult( string failureCause )
            : this()
        {
            this.AddFailureCause( failureCause );
        }

        /// <summary>
        /// Add a failure cause to this ConfigurationResult instance. This will mark the configuration change as failed.
        /// </summary>
        /// <param name="failureCause">Descriptive cause of why the change has failed.</param>
        internal void AddFailureCause( string failureCause )
        {
            if( failureCause == null ) throw new ArgumentNullException( "failureCause" );

            _causes.Add( failureCause );
        }

        public static implicit operator bool( ConfigurationResult result )
        {
            return result != null && result.IsSuccessful;
        }
    }
}
