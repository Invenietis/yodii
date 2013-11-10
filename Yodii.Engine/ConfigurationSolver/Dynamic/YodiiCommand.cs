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
        Dictionary<PluginData, IPluginInfo> _availablePlugins;
        Dictionary<ServiceData, IServiceInfo> _services;
        List<ServiceRootData> _servicesRootData;
    }
}
