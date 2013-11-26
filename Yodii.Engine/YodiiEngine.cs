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
    //Can do : gerer 
    public class YodiiEngine : IYodiiEngine
    {
        IDiscoveredInfo _discoveredInfo;
        readonly ConfigurationManager _manager;
        ConfigurationSolver _currentSolver;
        readonly List<YodiiCommand> _yodiiCommands;
        readonly IYodiiEngineHost _host;

        bool _isStart;
        ConfigurationSolver _virtualSolver;

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
            if( _isStart )
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
            if( _isStart )
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

        public bool IsStart
        {
            get { return _isStart; }
        }

        public IYodiiEngineResult Start()
        {
            _isStart = true;
            return null;
        }

        public void Stop()
        {
            _isStart = false;
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
