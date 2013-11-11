using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class ConfigState
    {
        internal List<ServiceRootData> _servicesRootData;
        internal Dictionary<IServiceInfo, ServiceData> _services;
        internal Dictionary<IPluginInfo, PluginData> _plugins;
        readonly FinalConfiguration _finalConfig;
        readonly IDiscoveredInfo _discoveredInfo;

        internal ConfigState( FinalConfiguration finalConfig, IDiscoveredInfo info )
        {
            _finalConfig = finalConfig;
            _discoveredInfo = info;
        }

        //Before static resolution, we save configuration state + discoverer info
        FinalConfiguration OriginalFinalConfig
        {
            get { return _finalConfig; }
        }
        IDiscoveredInfo OriginalDiscoveredInfo
        {
            get { return _discoveredInfo; }
        }

        //Before dynamic resolution, we save the 3 collections produced by the static resolution
        //TO DO: AsReadOnly()
        List<ServiceRootData> ServicesRootData
        {
            get { return _servicesRootData; }
            set { _servicesRootData = value; }
        }

        Dictionary<IServiceInfo, ServiceData> Services
        {
            get { return _services; }
            set { _services = value; }
        }
        Dictionary<IPluginInfo, PluginData> Plugins
        {
            get { return _plugins; }
            set { _plugins = value; }
        }

        //Possibly, after dynamic resolution we save something as well
    }
}
