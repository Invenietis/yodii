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
        readonly IYodiiEngine _engine;
        readonly IConfigurationFailureResult _configurationFailureResult;
        readonly IStaticFailureResult _staticFailureResult;
        readonly IDynamicFailureResult _hostFailureResult;
        readonly IReadOnlyList<IPluginInfo> _pluginCulprits;
        readonly IReadOnlyList<IServiceInfo> _serviceCulprits;

        /// <summary>
        /// Static failure resolution constructor.
        /// </summary>
        public YodiiEngineResult( Dictionary<string, ServiceData> services, Dictionary<string, PluginData> plugins, List<PluginData> blockingPlugins, List<ServiceData> blockingServices, YodiiEngine engine )
        {
            Debug.Assert( blockingPlugins != null || blockingServices != null, "At least one must not be null." );
            Debug.Assert( services != null && plugins != null );
            Debug.Assert( engine != null );

            var allP = plugins.Values.Select( p => new SolvedPluginSnapshot( p ) ).ToDictionary( p => p.PluginInfo );
            var blkP = blockingPlugins == null 
                            ? CKReadOnlyListEmpty<IStaticSolvedPlugin>.Empty 
                            : blockingPlugins.Select( p => allP[p.PluginInfo] ).ToReadOnlyList();
            _pluginCulprits = blkP.Select( ps => ps.PluginInfo ).ToReadOnlyList();

            var allS = services.Values.Select( s => new SolvedServiceSnapshot( s ) ).ToDictionary( s => s.ServiceInfo.ServiceFullName  );
            var blkS = blockingServices == null
                            ? CKReadOnlyListEmpty<IStaticSolvedService>.Empty
                            : blockingServices.Select( s => allS[s.ServiceInfo.ServiceFullName] ).ToReadOnlyList();
            _serviceCulprits = blkS.Select( ss => ss.ServiceInfo ).ToReadOnlyList();
            _engine = engine;
            _staticFailureResult = new StaticFailureResult( new StaticSolvedConfiguration( allP.Values.ToReadOnlyList(), allS.Values.ToReadOnlyList() ), blkP, blkS );
        }

        internal YodiiEngineResult( Dictionary<string, ServiceData> services, Dictionary<string, PluginData> plugins, IEnumerable<Tuple<IPluginInfo, Exception>> errorInfo, YodiiEngine engine )
        {
            Debug.Assert( services != null && plugins != null );
            Debug.Assert( errorInfo != null && errorInfo.Any() );
            Debug.Assert( engine != null );

            var allP = plugins.Values.Select( p => new SolvedPluginSnapshot( p ) ).ToDictionary( ps => ps.PluginInfo );
            var allS = services.Values.Select( s => new SolvedServiceSnapshot( s ) ).ToReadOnlyList();

            var errors = errorInfo.Select( e => new PluginRuntimeError( allP[e.Item1], e.Item2 ) ).ToReadOnlyList();
            _pluginCulprits = errors.Select( e => e.Plugin.PluginInfo ).ToReadOnlyList();
            _serviceCulprits = _pluginCulprits.Select( p => p.Service ).Where( s => s != null ).ToReadOnlyList();
            _engine = engine;

            _hostFailureResult = new DynamicFailureResult( new DynamicSolvedConfiguration( allP.Values.ToReadOnlyList(), allS ), errors );
        }

        internal YodiiEngineResult( IConfigurationFailureResult configurationFailureResult, YodiiEngine engine )
        {
            Debug.Assert( configurationFailureResult.FailureReasons != null && configurationFailureResult.FailureReasons.Count > 0 );

            _engine = engine;
            _configurationFailureResult = configurationFailureResult;
        }

        internal YodiiEngineResult( IConfigurationFailureResult configurationFailureResult )
            :this(configurationFailureResult, null)
        {
        }
        public bool Success { get { return _configurationFailureResult == null && _staticFailureResult == null && _hostFailureResult == null; } }

        public IConfigurationFailureResult ConfigurationFailureResult
        {
            get { return _configurationFailureResult; }
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
