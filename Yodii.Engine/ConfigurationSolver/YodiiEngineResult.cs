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
            Debug.Assert( services != null || plugins != null );

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
            _staticFailureResult = new StaticFailureResult( new StaticSolvedConfiguration(AllPlugins, AllServices), BlockingPlugins, BlockingServices );
        }

        internal YodiiEngineResult( Dictionary<IServiceInfo, ServiceData> services, Dictionary<IPluginInfo, PluginData> plugins, IEnumerable<Tuple<IPluginInfo, Exception>> errorInfo )
        {
            Debug.Assert( errorInfo.Any() );

            List<IDynamicSolvedPlugin> dynamicPlugins = new List<IDynamicSolvedPlugin>();
            List<IDynamicSolvedService> dynamicServices = new List<IDynamicSolvedService>();
            List<PluginRuntimeError> runtimeErrors = new List<PluginRuntimeError>();

            foreach ( ServiceData s in services.Values )
            {
                dynamicServices.Add( new SolvedServiceSnapshot( s ) );
            }

            foreach ( PluginData p in plugins.Values )
            {
                dynamicPlugins.Add( new SolvedPluginSnapshot( p ) );
            }

            for(int i = 0; i < errorInfo.Count(); i++)
            {
                IDynamicSolvedPlugin pluginHasError = dynamicPlugins.FirstOrDefault( dynamicPlugin => dynamicPlugin.PluginInfo == errorInfo.ElementAt(i).Item1 );
                if(pluginHasError != null)
                {
                    runtimeErrors.Add( new PluginRuntimeError( pluginHasError, errorInfo.ElementAt( i ).Item2 ) );
                }
            }
            _hostFailureResult = new DynamicFailureResult( new DynamicSolvedConfiguration( dynamicPlugins, dynamicServices ), runtimeErrors.AsReadOnlyList() );
        }
       
        public bool Success { get { return _staticFailureResult == null && _hostFailureResult == null; } }

        public IConfigurationManagerFailureResult ConfigurationManagerFailureResult
        {
            get { return null; }
        }

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
