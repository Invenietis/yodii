using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Discoverer
{
    internal class DiscoveredInfo : IDiscoveredInfo
    {
        readonly StandardDiscoverer _discoverer;
        readonly string _errorMessage;
        readonly int _version;
        readonly IReadOnlyList<IAssemblyInfo> _assemblies;
        readonly IReadOnlyList<IPluginInfo> _allPlugins;
        readonly IReadOnlyList<IServiceInfo> _allServices;

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

        internal DiscoveredInfo( IReadOnlyList<IAssemblyInfo> assemblies, StandardDiscoverer discoverer )
        {
            _assemblies = assemblies;
            _allPlugins = _assemblies.SelectMany( p => p.Plugins ).ToReadOnlyList();
            _allServices = _assemblies.SelectMany( s => s.Services ).ToReadOnlyList();
            _discoverer = discoverer;
            _version = _discoverer.CurrentVersion;
        }

        public IReadOnlyList<IPluginInfo> PluginInfos
        {
            get { return _allPlugins; }
        }

        public IReadOnlyList<IServiceInfo> ServiceInfos
        {
            get { return _allServices; }
        }

        public IReadOnlyList<IAssemblyInfo> AssemblyInfos
        {
            get { return _assemblies; }
        }
    }
}
