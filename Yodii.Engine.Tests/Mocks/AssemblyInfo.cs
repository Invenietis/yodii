using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    public class AssemblyInfo : IAssemblyInfo
    {
        readonly Uri _location;
        readonly AssemblyName _assemblyName;

        internal AssemblyInfo( string assemblyUri )
        {
            _location = new Uri( assemblyUri );
        }

        public Uri AssemblyLocation
        {
	        get { return _location; }
        }

        public AssemblyName AssemblyName { get { return _assemblyName; } }

        public IReadOnlyList<IPluginInfo> Plugins
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IServiceInfo> Services
        {
            get { throw new NotImplementedException(); }
        }
    }
}
