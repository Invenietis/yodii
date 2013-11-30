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
        LiveInfo _liveInfo;

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
                CurrentSolver = _virtualSolver;
            }
            _virtualSolver = null;
            return new SuccessYodiiEngineResult();
        }

        internal ConfigurationSolver CurrentSolver
        {
            set
            {
                _currentSolver = value;
                RaisePropertyChanged("IsRunning");
            }
        }

        public IDiscoveredInfo DiscoveredInfo
        {
            get { return _discoveredInfo; }
            private set
            {
                _discoveredInfo = value;
                RaisePropertyChanged();
            }
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
                CurrentSolver = solver;
            }
            return result;
        }

        private void InitializeLiveInfo()
        {
            Debug.Assert(_liveInfo == null);
            _liveInfo = new LiveInfo();
        }

        public void Stop()
        {
            CurrentSolver = null;
        }

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if( info == null ) throw new ArgumentNullException( "info" );
            if( IsRunning )
            {
                ConfigurationSolver solver = new ConfigurationSolver();
                IYodiiEngineResult result = solver.StaticResolution( _manager.FinalConfiguration, info );
                if( !result.Success ) return result;
                var toDo = solver.DynamicResolution( _yodiiCommands );
                var errors = _host.Apply( toDo.Item1, toDo.Item2, toDo.Item3 );
                if( errors != null && errors.Any() )
                {
                    result = solver.CreateDynamicFailureResult( errors );
                }
                if( result.Success )
                {
                    DiscoveredInfo = info;
                }
                return result;
            }
            else
            {
                DiscoveredInfo = info;
                return new SuccessYodiiEngineResult();
            }
        }

        void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public ILiveInfo LiveInfo
        {
            get { throw new NotImplementedException(); }
        }
        public IYodiiEngineHost Host
        {
            get { return _host; }
        }
        //public List<YodiiCommand> YodiiCommands
        //{
        //    get { return _yodiiCommands; }
        //}
    }
}
