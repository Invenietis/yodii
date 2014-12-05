using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    class CancellationInfo : IPluginHostApplyCancellationInfo
    {
        public CancellationInfo( IPluginInfo p, bool isLoadError = false )
        {
            Plugin = p;
            IsLoadError = isLoadError;
        }

        public IPluginInfo Plugin { get; private set; }

        public bool IsLoadError { get; private set; }

        public bool IsStartCanceled { get; set; }

        public bool IsStopCanceled { get { return !IsLoadError && !IsStartCanceled; } }

        public string ErrorMessage { get; set; }

        public Exception Error { get; set; }
    }
}
