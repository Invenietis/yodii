using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    public class ConfigurationFailureResult : IConfigurationFailureResult
    {
        readonly List<string> _failureReasons;

        internal ConfigurationFailureResult()
        {
            _failureReasons = new List<string>();
        }

        internal ConfigurationFailureResult( string reason )
        {
            _failureReasons = new List<string>();
            _failureReasons.Add( reason );
        }

        internal ConfigurationFailureResult( IReadOnlyList<string> reasons )
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
