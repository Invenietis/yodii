using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;

namespace Yodii.Model
{
    public class ConfigurationManagerFailureResult : IConfigurationManagerFailureResult
    {
        readonly List<ConfigurationConflict> _blockingItems;
        readonly List<string> _failureReasons;

        internal ConfigurationManagerFailureResult()
        {
            _blockingItems = new List<ConfigurationConflict>();
            _failureReasons = new List<string>();
        }

        internal void addBlokingItem( ConfigurationConflict conflict )
        {
            Debug.Assert( _blockingItems.Any( c => c.DisablerItem != conflict.DisablerItem && c.RunnerItem != conflict.RunnerItem ) );
            Debug.Assert( _blockingItems.Any( c => c.DisablerItem.Status == ConfigurationStatus.Disable && ( c.RunnerItem.Status == ConfigurationStatus.Runnable || c.RunnerItem.Status == ConfigurationStatus.Running ) ) );
            _blockingItems.Add( conflict );
        }

        internal void addFailureReason( string reason )
        {
            _failureReasons.Add( reason );
        }

        public bool Success
        {
            get { return _blockingItems.Count == 0 && _failureReasons.Count == 0; }
        }

        #region IConfigurationManagerFailureResult Members

        public IReadOnlyList<ConfigurationConflict> BlockingItems
        {
            get { return _blockingItems.AsReadOnlyList(); }
        }

        public IReadOnlyList<string> FailureReasons
        {
            get { return _failureReasons.AsReadOnlyList(); }
        }

        #endregion
    }
}
