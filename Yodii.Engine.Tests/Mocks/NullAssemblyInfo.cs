using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    internal class NullAssemblyInfo : IAssemblyInfo
    {
        #region IAssemblyInfo Members

        public string AssemblyFileName
        {
            get { throw new NotImplementedException(); }
        }

        public bool HasPluginsOrServices
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IPluginInfo> Plugins
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IServiceInfo> Services
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
