using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    public class EmptyDiscoveredInfo : IDiscoveredInfo
    {

        public static readonly IDiscoveredInfo Empty = new EmptyDiscoveredInfo();

        EmptyDiscoveredInfo()
        {
        }

        public IReadOnlyList<IServiceInfo> ServiceInfos
        {
            get { return CKReadOnlyListEmpty<IServiceInfo>.Empty; }
        }

        public IReadOnlyList<IPluginInfo> PluginInfos
        {
            get { return CKReadOnlyListEmpty<IPluginInfo>.Empty; }
        }

        public IReadOnlyList<IAssemblyInfo> AssemblyInfos
        {
            get { return CKReadOnlyListEmpty<IAssemblyInfo>.Empty; }
        }
    }
}
