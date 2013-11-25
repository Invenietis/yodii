using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;

namespace Yodii.Engine
{
    public class ConfigurationManagerFailureResult : IConfigurationManagerFailureResult
    {
        readonly List<string> _failureReasons;

        internal ConfigurationManagerFailureResult()
        {
            _failureReasons = new List<string>();
        }

        internal ConfigurationManagerFailureResult( string reason )
        {
            _failureReasons = new List<string>();
            _failureReasons.Add( reason );
        }

        internal ConfigurationManagerFailureResult( IReadOnlyList<string> reasons )
        {
            _failureReasons = reasons.ToList();
        }

        internal void addFailureReason( string reason )
        {
            _failureReasons.Add( reason );
        }

        public bool Success
        {
            get { return _failureReasons.Count == 0; }
        }

        #region IConfigurationManagerFailureResult Members

        public IReadOnlyList<string> FailureReasons
        {
            get { return _failureReasons.AsReadOnlyList(); }
        }

        #endregion
    }
}
