using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    class ConfigurationFailureResult : IConfigurationFailureResult
    {
        readonly IReadOnlyList<string> _failureReasons;

        /// <summary>
        /// Initializes a new success result.
        /// </summary>
        internal ConfigurationFailureResult()
        {
        }

        /// <summary>
        /// Initializes a failure result from the FillConfiguration: only one
        /// blocking condition can be detected.
        /// </summary>
        /// <param name="reason"></param>
        internal ConfigurationFailureResult( string reason )
        {
            Debug.Assert( !String.IsNullOrWhiteSpace( reason ) );
            _failureReasons = new CKReadOnlyListMono<string>( reason );
        }

        /// <summary>
        /// Initializes a failure result due to cancellations by external code (from ConfigurationChanging event).
        /// </summary>
        /// <param name="reasons">Multiple reasons.</param>
        internal ConfigurationFailureResult( IReadOnlyList<string> reasons )
        {
            Debug.Assert( reasons != null && reasons.Count > 0 );
            _failureReasons = reasons;
        }

        public bool Success
        {
            get { return _failureReasons == null; }
        }

        public IReadOnlyList<string> FailureReasons
        {
            get { return _failureReasons; }
        }

    }
}
