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
            Debug.Assert( blockingPlugins != null || blockingServices != null );
            List<IStaticSolvedService> AllServices = new List<IStaticSolvedService>();
            List<IStaticSolvedPlugin> AllPlugins = new List<IStaticSolvedPlugin>();

            List<IStaticSolvedService> BlockingServices = new List<IStaticSolvedService>();
            List<IStaticSolvedPlugin> BlockingPlugins = new List<IStaticSolvedPlugin>();

            foreach ( ServiceData s in services.Values )
            {
                AllServices.Add( new SolvedServiceSnapshot( s ) );
            }

            foreach ( PluginData p in plugins.Values )
            {
                AllPlugins.Add( new SolvedPluginSnapshot( p ) );
            }
            if ( blockingPlugins != null )
            {
                foreach ( PluginData pb in blockingPlugins )
                {
                    BlockingPlugins.Add( new SolvedPluginSnapshot( pb ) );
                }
            }
            else if ( blockingServices != null )
            {
                foreach ( ServiceData sb in blockingServices )
                {
                    BlockingServices.Add( new SolvedServiceSnapshot( sb ) );
                }
            }
            StaticFailureResult _staticFailureResult = new StaticFailureResult( new StaticSolvedConfiguration(AllPlugins, AllServices), BlockingPlugins, BlockingServices );
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
