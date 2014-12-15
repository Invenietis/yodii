using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    class YodiiEngineResult : IYodiiEngineStaticOnlyResult
    {
        readonly IYodiiEngine _engine;
        readonly IConfigurationFailureResult _configurationFailureResult;
        readonly IStaticFailureResult _staticFailureResult;
        readonly IDynamicFailureResult _hostFailureResult;
        readonly IReadOnlyList<IPluginInfo> _pluginCulprits;
        readonly IReadOnlyList<IServiceInfo> _serviceCulprits;
        readonly IStaticSolvedConfiguration _staticOnlyResultConfiguration;

        /// <summary>
        /// Static only success resolution constructor.
        /// </summary>
        public YodiiEngineResult( IConfigurationSolver solver, YodiiEngine engine )
        {
            Debug.Assert( solver != null );
            Debug.Assert( engine != null );

            var allP = solver.AllPlugins.Select( p => new SolvedPluginSnapshot( p ) ).ToDictionary( p => p.PluginInfo );
            var allS = solver.AllServices.Select( s => new SolvedServiceSnapshot( s ) ).ToDictionary( s => s.ServiceInfo.ServiceFullName );
            _staticOnlyResultConfiguration = new StaticSolvedConfiguration( allP.Values.ToReadOnlyList(), allS.Values.ToReadOnlyList() );
            _pluginCulprits = CKReadOnlyListEmpty<IPluginInfo>.Empty;
            _serviceCulprits = CKReadOnlyListEmpty<IServiceInfo>.Empty;
            _engine = engine;
        }

        /// <summary>
        /// Static failure resolution constructor.
        /// </summary>
        public YodiiEngineResult( IConfigurationSolver solver, List<PluginData> blockingPlugins, List<ServiceData> blockingServices, YodiiEngine engine )
        {
            Debug.Assert( blockingPlugins != null || blockingServices != null, "At least one must not be null." );
            Debug.Assert( solver != null );
            Debug.Assert( engine != null );

            var allP = solver.AllPlugins.Select( p => new SolvedPluginSnapshot( p ) ).ToDictionary( p => p.PluginInfo );
            var allS = solver.AllServices.Select( s => new SolvedServiceSnapshot( s ) ).ToDictionary( s => s.ServiceInfo.ServiceFullName );
            _staticOnlyResultConfiguration = new StaticSolvedConfiguration( allP.Values.ToReadOnlyList(), allS.Values.ToReadOnlyList() );

            var blkP = blockingPlugins == null
                            ? CKReadOnlyListEmpty<IStaticSolvedPlugin>.Empty
                            : blockingPlugins.Select( p => allP[p.PluginInfo] ).ToReadOnlyList();
            _pluginCulprits = blkP.Select( ps => ps.PluginInfo ).ToReadOnlyList();

            var blkS = blockingServices == null
                            ? CKReadOnlyListEmpty<IStaticSolvedService>.Empty
                            : blockingServices.Select( s => allS[s.ServiceInfo.ServiceFullName] ).ToReadOnlyList();
            _serviceCulprits = blkS.Select( ss => ss.ServiceInfo ).ToReadOnlyList();

            _engine = engine;
            _staticFailureResult = new StaticFailureResult( _staticOnlyResultConfiguration, blkP, blkS );
        }

        /// <summary>
        /// Dynamic failure constructor.
        /// </summary>
        internal YodiiEngineResult( IConfigurationSolver solver, IReadOnlyList<IPluginHostApplyCancellationInfo> applyErrors, YodiiEngine engine )
        {
            Debug.Assert( solver != null );
            Debug.Assert( applyErrors != null && applyErrors.Any() );
            Debug.Assert( engine != null );

            var allP = solver.AllPlugins.Select( p => new SolvedPluginSnapshot( p ) ).ToDictionary( ps => ps.PluginInfo );
            var allS = solver.AllServices.Select( s => new SolvedServiceSnapshot( s ) ).ToReadOnlyList();

            var errors = applyErrors.Select( e => new PluginRuntimeError( allP[e.Plugin], e ) ).ToReadOnlyList();
            _pluginCulprits = errors.Select( e => e.Plugin.PluginInfo ).ToReadOnlyList();
            _serviceCulprits = _pluginCulprits.Select( p => p.Service ).Where( s => s != null ).ToReadOnlyList();
            _engine = engine;

            _hostFailureResult = new DynamicFailureResult( new DynamicSolvedConfiguration( allP.Values.ToReadOnlyList(), allS ), errors );
        }

        /// <summary>
        /// Configuration failure constructor.
        /// </summary>
        internal YodiiEngineResult( IConfigurationFailureResult configurationFailureResult, YodiiEngine engine )
        {
            Debug.Assert( configurationFailureResult.FailureReasons != null && configurationFailureResult.FailureReasons.Count > 0 );

            _engine = engine;
            _configurationFailureResult = configurationFailureResult;
        }

        public IYodiiEngine Engine { get { return _engine; } }

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

        IStaticSolvedConfiguration IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration
        {
            get { return _staticOnlyResultConfiguration; }
        }
    }
}
