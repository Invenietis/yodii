using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    public class YodiiEngine : IYodiiEngine
    {
        IDiscoveredInfo _discoveredInfo;
        readonly ConfigurationManager _manager;
        ConfigurationSolver _currentSolver;
        readonly List<YodiiCommand> _yodiiCommands;
        readonly IYodiiEngineHost _host;

        ConfigurationSolver _virtualSolver;

        public YodiiEngine( IYodiiEngineHost host )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _host = host;
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new List<YodiiCommand>();
        }

        internal IYodiiEngineResult StaticResolution( FinalConfiguration finalConfiguration )
        {
            Debug.Assert( _virtualSolver == null );
            _virtualSolver = new ConfigurationSolver();
            IYodiiEngineResult result = new YodiiEngineResult( _virtualSolver.StaticResolution( finalConfiguration, _discoveredInfo ) );
            if( !result.Success ) _virtualSolver = null;
            return result;
        }

        internal IYodiiEngineResult DynamicResolution()
        {
            Debug.Assert( _virtualSolver != null );
            var toDo = _virtualSolver.DynamicResolution( _yodiiCommands );
            var errors = _host.Apply( toDo.Item1, toDo.Item2, toDo.Item3 );
            if( errors != null && errors.Any() )
            {
                YodiiEngineResult result = new YodiiEngineResult( _virtualSolver.CreateDynamicFailureResult( errors ) );
                _virtualSolver = null;
                return result;
            }
            _currentSolver = _virtualSolver;
            _virtualSolver = null;
        }

        public IDiscoveredInfo DiscoveredInfo
        {
            get { return _discoveredInfo; }
        }

        public ConfigurationManager ConfigurationManager
        {
            get { return _manager; }
        }

        public IYodiiEngineResult Start()
        {
        }

        public void Stop()
        {
        }

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
 
        }

        void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            throw new NotImplementedException();
        }

    }
}
