using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;
using CK.Core;
using System.Collections.ObjectModel;

namespace Yodii.Engine
{
    public class YodiiEngine : IYodiiEngine
    {
        readonly ConfigurationManager _manager;
        readonly IYodiiEngineHost _host;
        readonly LiveInfo _liveInfo;
        readonly YodiiCommands _yodiiCommands;

        IDiscoveredInfo _discoveredInfo;
        ConfigurationSolver _currentSolver;

        class YodiiCommands : ObservableCollection<YodiiCommand>, IObservableReadOnlyList<YodiiCommand>
        {
            public YodiiCommands( IEnumerable<YodiiCommand> persistedCommands )
            {
                if( persistedCommands != null ) this.AddRange( persistedCommands );
            }

            public void Merge( IReadOnlyList<YodiiCommand> newCommands )
            {
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public YodiiEngine( IYodiiEngineHost host, IEnumerable<YodiiCommand> persistedCommands = null )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _host = host;
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new YodiiCommands( persistedCommands );
            _liveInfo = new LiveInfo( this );
        }

        internal Tuple<IYodiiEngineResult,ConfigurationSolver> StaticResolution( FinalConfiguration finalConfiguration )
        {
            Debug.Assert( IsRunning );

            ConfigurationSolver temporarySolver = new ConfigurationSolver();
            IYodiiEngineResult result =  temporarySolver.StaticResolution( finalConfiguration, _discoveredInfo );
            if( !result.Success ) temporarySolver = null;
            return Tuple.Create( result, temporarySolver );
        }

        IYodiiEngineResult DoDynamicResolution( ConfigurationSolver temporarySolver, YodiiCommand cmd, Action onPreSuccess = null )
        {
            Debug.Assert( temporarySolver == null || cmd == null );

            var solver = temporarySolver ?? _currentSolver;

            var dynResult = solver.DynamicResolution( _yodiiCommands, cmd );
            var errors = _host.Apply( dynResult.Disabled, dynResult.Stopped, dynResult.Running );
            if( errors != null && errors.Any() )
            {
                IYodiiEngineResult result =  solver.CreateDynamicFailureResult( errors );
                _liveInfo.UpdateRuntimeErrors( errors );
                return result;
            }
            // Success:
            if( onPreSuccess != null ) onPreSuccess();
            bool wasRunning = _currentSolver != null;
            if( _currentSolver != solver ) _currentSolver = solver;
            if( cmd != null ) _yodiiCommands.Merge( dynResult.Commands );
            _currentSolver.UpdateNewResultInLiveInfo( _liveInfo );
            if( !wasRunning ) RaisePropertyChanged( "IsRunning" );
            return new SuccessYodiiEngineResult();
        }

        internal IYodiiEngineResult OnConfigurationChanging( ConfigurationSolver temporarySolver )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( temporarySolver, null );
        }

        internal IYodiiEngineResult AddYodiiCommand( YodiiCommand cmd )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( null, cmd );
        }

        public IYodiiEngineResult Start()
        {
            if( !IsRunning )
            {
                ConfigurationSolver solver = new ConfigurationSolver();
                IYodiiEngineResult result = solver.StaticResolution( _manager.FinalConfiguration, _discoveredInfo );
                if( !result.Success ) return result;
                return DoDynamicResolution( solver, null );
            }
            return new SuccessYodiiEngineResult();
        }

        public IDiscoveredInfo DiscoveredInfo
        {
            get { return _discoveredInfo; }
            private set
            {
                if( _discoveredInfo != value )
                {
                    _discoveredInfo = value;
                    RaisePropertyChanged();
                }
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

        public void Stop()
        {
            if( IsRunning )
            {
                _liveInfo.Clear();
                _currentSolver = null;
                RaisePropertyChanged( "IsRunning" );
            }
        }

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if( info == null ) throw new ArgumentNullException( "info" );
            if( info == _discoveredInfo ) return new SuccessYodiiEngineResult();

            if( IsRunning )
            {
                ConfigurationSolver solver = new ConfigurationSolver();
                IYodiiEngineResult result = solver.StaticResolution( _manager.FinalConfiguration, info );
                if( !result.Success ) return result;

                return DoDynamicResolution( solver, null, () => DiscoveredInfo = info );
            }
            else DiscoveredInfo = info;
            return new SuccessYodiiEngineResult();
        }

        public IObservableReadOnlyList<YodiiCommand> YodiiCommands
        {
            get { return _yodiiCommands; }
        }

        void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public ILiveInfo LiveInfo
        {
            get { return _liveInfo; }
        }

        public IYodiiEngineHost Host
        {
            get { return _host; }
        }
    }
}
