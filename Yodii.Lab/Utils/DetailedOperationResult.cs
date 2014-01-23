using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Lab.Utils
{
    /// <summary>
    /// Operation result (success or failure), with a descriptive string.
    /// </summary>
    public class DetailedOperationResult
    {
        /// <summary>
        /// Description of the reason behind the operation success/failure.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Whather the operation was successful.
        /// </summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        /// Creates a new instance of DetailedOperationResult.
        /// </summary>
        /// <param name="isSuccessful">Whether the operation was successful.</param>
        /// <param name="reason">Reason behind the operation.</param>
        public DetailedOperationResult( bool isSuccessful = true, string reason = "" )
        {
            Reason = reason;
            IsSuccessful = isSuccessful;
        }

        /// <summary>
        /// Implicit bool operator.
        /// </summary>
        /// <param name="result">object to consider</param>
        /// <returns>True if result is successful.</returns>
        public static implicit operator bool( DetailedOperationResult result )
        {
            return result.IsSuccessful;
        }
    }
}
