using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal abstract class YodiiCommand
    {
        internal Dictionary<PluginData, ILivePluginInfo> _availablePlugins;
        internal Dictionary<ServiceData, ILiveServiceInfo> _services;
        internal Object _caller;

        internal Object ObjectCaller { get { return _caller; } }
    }
}
