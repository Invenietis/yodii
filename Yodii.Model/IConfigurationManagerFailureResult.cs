using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Failure result during Engine configuration.
    /// </summary>
    public interface IConfigurationFailureResult
    {
        /// <summary>
        /// Reasons for failure.
        /// </summary>
        IReadOnlyList<string> FailureReasons { get; }
    }
}
