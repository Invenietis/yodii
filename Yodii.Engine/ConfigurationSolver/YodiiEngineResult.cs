using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    class YodiiEngineResult : IYodiiEngineResult
    {
        readonly IStaticFailureResult _staticFailureResult;
        readonly IDynamicFailureResult _hostFailureResult;
        readonly IReadOnlyList<IPluginInfo> _pluginCulprits;
        readonly IReadOnlyList<IServiceInfo> _serviceCulprits;

        /// <summary>
        /// Static failure resoltion constructor.
        /// </summary>
        /// <param name="services"></param>
        /// <param name="plugins"></param>
        /// <param name="blockingPlugins"></param>
        /// <param name="blockingServices"></param>
        public YodiiEngineResult( Dictionary<IServiceInfo, ServiceData> services, Dictionary<IPluginInfo, PluginData> plugins, List<PluginData> blockingPlugins, List<ServiceData> blockingServices )
        {
            throw new NotImplementedException();
        }

        internal YodiiEngineResult( IDynamicFailureResult hostFailureResult )
        {
            _hostFailureResult = hostFailureResult;
        }

        public bool Success { get { return _staticFailureResult == null && _hostFailureResult == null; } }


        public IStaticFailureResult StaticFailureResult
        {
            get { return _staticFailureResult; }
        }

        public IDynamicFailureResult HostFailureResult
        {
            get { return _hostFailureResult; }
        }

        public IReadOnlyList<IPluginInfo> PluginCulprits
        {
            get { return _pluginCulprits; }
        }

        public IReadOnlyList<IServiceInfo> ServiceCulprits
        {
            get { return _serviceCulprits; }
        }
    }
}
