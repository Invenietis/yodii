using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer
{
    internal abstract class DiscoveredInfo : IDiscoveredInfo
    {
        StandardDiscoverer _discoverer;
        string _errorMessage;
        int _version;

        public bool HasError
        {
            get { return ErrorMessage != null && ErrorMessage.Length > 0; }
        }

        public int LastChangedVersion
        {
            get { return _version; }
        }

        public bool HasChanged
        {
            get { return _version == _discoverer.CurrentVersion; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

        internal StandardDiscoverer Discoverer { get { return _discoverer; } }

        protected DiscoveredInfo( StandardDiscoverer discoverer )
        {
            _discoverer = discoverer;
            _version = _discoverer.CurrentVersion;
        }

        public IReadOnlyList<IServiceInfo> ServiceInfos
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IPluginInfo> PluginInfos
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IAssemblyInfo> AssemblyInfos
        {
            get { throw new NotImplementedException(); }
        }
    }
}
