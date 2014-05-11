using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer
{
    internal sealed class AssemblyInfo : IAssemblyInfo
    {
        readonly Uri _location;
        readonly string _errorMessage;
        readonly IReadOnlyList<ServiceInfo> _services;
        readonly IReadOnlyList<PluginInfo> _plugins;

        internal AssemblyInfo( Uri location, string errorMessage )
        {
            Debug.Assert( location != null && errorMessage != null );
            _location = location;
            _errorMessage = errorMessage;
        }

        internal AssemblyInfo( Uri location, IReadOnlyList<ServiceInfo> services, IReadOnlyList<PluginInfo> plugins )
        {
            Debug.Assert( location != null && services != null && plugins != null );
            _location = location;
            _services = services;
            _plugins = plugins;
        }
        internal AssemblyInfo( Uri location )
        {
            _location = location;
        }
        public Uri AssemblyLocation
        {
            get { return _location; }
        }

        public bool HasErrorMessage { get { return _errorMessage != null; } }

        public string ErrorMessage { get { return _errorMessage; } }

        public IReadOnlyList<IServiceInfo> Services { get { return _services; } }

        public IReadOnlyList<IPluginInfo> Plugins { get { return _plugins; } }
    }
}
