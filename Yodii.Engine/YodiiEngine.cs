using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    public class YodiiEngine : IYodiiEngine
    {
        readonly ConfigurationManager _manager;
        readonly List<YodiiCommand> _yodiiCommands;
        readonly IYodiiEngineHost _host;

        IDiscoveredInfo _discoveredInfo;
        ConfigurationSolver _virtualSolver;
        ConfigurationSolver _currentSolver;

        public event PropertyChangedEventHandler PropertyChanged;

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
            if( IsRunning )
            {
                Debug.Assert( _virtualSolver == null );
                _virtualSolver = new ConfigurationSolver();
                IYodiiEngineResult result =  _virtualSolver.StaticResolution( finalConfiguration, _discoveredInfo );
                if( !result.Success ) _virtualSolver = null;
                return result;
            }
            return new SuccessYodiiEngineResult();
        }

        internal IYodiiEngineResult DynamicResolution()
        {
            if( IsRunning )
            {
                Debug.Assert( _virtualSolver != null );
                var toDo = _virtualSolver.DynamicResolution( _yodiiCommands );
                var errors = _host.Apply( toDo.Item1, toDo.Item2, toDo.Item3 );
                if( errors != null && errors.Any() )
                {
                    IYodiiEngineResult result =  _virtualSolver.CreateDynamicFailureResult( errors );
                    _virtualSolver = null;
                    return result;
                }
                _currentSolver = _virtualSolver;
            }
            _virtualSolver = null;
            return new SuccessYodiiEngineResult();
        }

        public IDiscoveredInfo DiscoveredInfo
        {
            get { return _discoveredInfo; }
        }

        public IConfigurationManager ConfigurationManager
        {
            get { return _manager; }
        }

        public bool IsRunning
        {
            get { return _currentSolver != null; }
        }

        public IYodiiEngineResult Start()
        {
            ConfigurationSolver solver = new ConfigurationSolver();
            IYodiiEngineResult result = solver.StaticResolution( _manager.FinalConfiguration, _discoveredInfo );
            if( !result.Success ) return result;
            var toDo = solver.DynamicResolution( _yodiiCommands );
            var errors = _host.Apply( toDo.Item1, toDo.Item2, toDo.Item3 );
            if( errors != null && errors.Any() )
            {
                result =  solver.CreateDynamicFailureResult( errors );
            }
            if( result.Success )
            {
                _currentSolver = solver;
            }
            return result;
        }

        public void Stop()
        {
            _currentSolver = null;
        }

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if ( info == null ) throw new ArgumentNullException( "DiscoveredInfo" );
            IYodiiEngineResult staticResult = StaticResolution( _manager.FinalConfiguration );
            if ( !staticResult.Success ) return staticResult;
            IYodiiEngineResult dynamicResult = DynamicResolution();
            return dynamicResult;
        }

        void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            throw new NotImplementedException();
        }

        public ILiveInfo LiveInfo
        {
            get { throw new NotImplementedException(); }
        }

    }
}
