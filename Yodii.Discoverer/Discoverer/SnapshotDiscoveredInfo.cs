using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Discoverer
{
    internal class DiscoveredInfo : IDiscoveredInfo
    {
        readonly IReadOnlyList<IAssemblyInfo> _assemblies;
        readonly IReadOnlyList<IPluginInfo> _allPlugins;
        readonly IReadOnlyList<IServiceInfo> _allServices;

        internal DiscoveredInfo( IReadOnlyList<IAssemblyInfo> assemblies )
        {
            _assemblies = assemblies;
            _allPlugins = _assemblies.SelectMany( p => p.Plugins ).ToReadOnlyList();
            _allServices = _assemblies.SelectMany( s => s.Services ).ToReadOnlyList();
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
