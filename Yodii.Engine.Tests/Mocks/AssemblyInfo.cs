using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    public class AssemblyInfo : IAssemblyInfo
    {
        readonly Uri _location;

        internal AssemblyInfo( string assemblyUri )
        {
            _location = new Uri( assemblyUri );
        }

        public Uri AssemblyLocation
        {
	        get { return _location; }
        }


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
