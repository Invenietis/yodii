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
        readonly YodiiCommandList _yodiiCommands;

        IDiscoveredInfo _discoveredInfo;
        ConfigurationSolver _currentSolver;

        class YodiiCommandList : ObservableCollection<YodiiCommand>, IObservableReadOnlyList<YodiiCommand>
        {
            public YodiiCommandList( IEnumerable<YodiiCommand> persistedCommands )
            {
                if( persistedCommands != null ) this.AddRange( persistedCommands );
            }

            public void Merge( IReadOnlyList<YodiiCommand> newCommands )
            {
                if( newCommands.Count == 0 )
                {
                    this.Clear();
                    return;
                }
                if( this.Count == 0 )
                {
                    this.AddRange( newCommands );
                    return;
                }

                if( this[0] != newCommands[0] ) this.InsertItem( 0, newCommands[0] );
                for( int i = 1; i < newCommands.Count; i++ )
                {
                    if( newCommands[i] != this[i] ) this.RemoveAt( i-- );
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public YodiiEngine( IYodiiEngineHost host, IEnumerable<YodiiCommand> persistedCommands = null )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _host = host;
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new YodiiCommandList( persistedCommands );
            _liveInfo = new LiveInfo( this );
        }

        internal Tuple<IYodiiEngineResult,ConfigurationSolver> StaticResolution( FinalConfiguration finalConfiguration )
        {
            Debug.Assert( IsRunning );
            return ConfigurationSolver.CreateAndApplyStaticResolution( finalConfiguration, _discoveredInfo );
        }

        IYodiiEngineResult DoDynamicResolution( ConfigurationSolver solver, Func<YodiiCommand, bool> existingCommandFilter, YodiiCommand cmd, Action onPreSuccess = null )
        {
            var dynResult = solver.DynamicResolution( existingCommandFilter != null ? _yodiiCommands.Where( existingCommandFilter ) : _yodiiCommands, cmd );
            var errors = _host.Apply( dynResult.Disabled, dynResult.Stopped, dynResult.Running );
            if( errors != null && errors.Any() )
            {
                IYodiiEngineResult result =  solver.CreateDynamicFailureResult( errors );
                _liveInfo.UpdateRuntimeErrors( errors );
                return result;
            }
            // Success:
            if( onPreSuccess != null ) onPreSuccess();
            bool wasStopped = _currentSolver == null;
            if( _currentSolver != solver ) _currentSolver = solver;

            _liveInfo.UpdateFrom( _currentSolver );
                        
            _yodiiCommands.Merge( dynResult.Commands );
            if( wasStopped ) RaisePropertyChanged( "IsRunning" );
            return SuccessYodiiEngineResult.Default;
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


        internal IYodiiEngineResult OnConfigurationChanging( ConfigurationSolver temporarySolver )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( temporarySolver, null, null );
        }

        internal IYodiiEngineResult AddYodiiCommand( YodiiCommand cmd )
        {
            Debug.Assert( IsRunning, "Cannot call this function when the engine is not running" );
            return DoDynamicResolution( _currentSolver, null, cmd );
        }

        internal IYodiiEngineResult RevokeYodiiCommandCaller( string callerKey )
        {
            Debug.Assert( callerKey != null );
            if( IsRunning )
            {
                return DoDynamicResolution( _currentSolver, cmd => cmd.CallerKey != callerKey, null );
            }
            else
            {
                _yodiiCommands.RemoveWhereAndReturnsRemoved( cmd => cmd.CallerKey == callerKey ).Count();
                return SuccessYodiiEngineResult.Default;
            }
        }

        public IYodiiEngineResult Start()
        {
            return Start( false, false );
        }

        public IYodiiEngineResult Start( bool revertServices, bool revertPlugins )
        {
            if( !IsRunning )
            {
                var r = ConfigurationSolver.CreateAndApplyStaticResolution( _manager.FinalConfiguration, _discoveredInfo, revertServices, revertPlugins );
                if( !r.Item1.Success ) return r.Item1;
                return DoDynamicResolution( r.Item2, null, null );
            }
            return SuccessYodiiEngineResult.Default;
        }

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if( info == null ) throw new ArgumentNullException( "info" );
            if( info == _discoveredInfo ) return SuccessYodiiEngineResult.Default;

            if( IsRunning )
            {
                var r = ConfigurationSolver.CreateAndApplyStaticResolution( _manager.FinalConfiguration, info );
                if( !r.Item1.Success ) return r.Item1;
                return DoDynamicResolution( r.Item2, null, null, () => DiscoveredInfo = info );
            }
            else DiscoveredInfo = info;
            return SuccessYodiiEngineResult.Default;
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
