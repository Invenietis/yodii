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
        readonly SuccessYodiiEngineResult _successResult;

        IDiscoveredInfo _discoveredInfo;
        ConfigurationSolver _currentSolver;
        
        class YodiiCommandList : ObservableCollection<YodiiCommand>, IObservableReadOnlyList<YodiiCommand>
        {
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
                while( this.Count > newCommands.Count ) this.RemoveAt( Count - 1 );
                Debug.Assert( this.Count == newCommands.Count );
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public YodiiEngine( IYodiiEngineHost host )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _successResult = new SuccessYodiiEngineResult( this );
            _host = host;
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new YodiiCommandList();
            _liveInfo = new LiveInfo( this );
        }

        internal SuccessYodiiEngineResult SuccessResult 
        { 
            get { return _successResult; } 
        }

        internal Tuple<IYodiiEngineStaticOnlyResult,ConfigurationSolver> StaticResolutionByConfigurationManager( FinalConfiguration finalConfiguration )
        {
            Debug.Assert( IsRunning );
            return ConfigurationSolver.CreateAndApplyStaticResolution( this, finalConfiguration, _discoveredInfo, false, false, false );
        }

        IYodiiEngineResult DoDynamicResolution( ConfigurationSolver solver, Func<YodiiCommand, bool> existingCommandFilter, YodiiCommand cmd, Action onPreSuccess = null )
        {
            var dynResult = solver.DynamicResolution( existingCommandFilter != null ? _yodiiCommands.Where( existingCommandFilter ) : _yodiiCommands, cmd );
            var errors = _host.Apply( dynResult.Disabled, dynResult.Stopped, dynResult.Running );
            if( errors != null && errors.Any() )
            {
                IYodiiEngineResult result =  solver.CreateDynamicFailureResult( errors );
                _liveInfo.UpdateRuntimeErrors( errors, solver.FindExistingPlugin );
                return result;
            }
            // Success:
            if( onPreSuccess != null ) onPreSuccess();
            bool wasStopped = _currentSolver == null;
            if( _currentSolver != solver ) _currentSolver = solver;

            _liveInfo.UpdateFrom( _currentSolver );
                        
            _yodiiCommands.Merge( dynResult.Commands );
            if( wasStopped ) RaisePropertyChanged( "IsRunning" );
            return _successResult;
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

        public IConfigurationManager Configuration
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
                return _successResult;
            }
        }

        public IYodiiEngineResult Start( IEnumerable<YodiiCommand> persistedCommands = null )
        {
            return Start( false, false );
        }

        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="DiscoveredInfo"/> and <see cref="Configuration"/>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <returns>The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.</returns>
        public IYodiiEngineStaticOnlyResult StaticResolutionOnly()
        {
            return StaticResolutionOnly( false, false );
        }

        /// <summary>
        /// Triggers the static resolution of the graph (with the current <see cref="DiscoveredInfo"/> and <see cref="Configuration"/>).
        /// This has no impact on the engine and can be called when <see cref="IsRunning"/> is false.
        /// </summary>
        /// <param name="revertServices">True to revert the list of the services (based on their <see cref="IServiceInfo.ServiceFullName"/>).</param>
        /// <param name="revertPlugins">True to revert the list of the plugins (based on their <see cref="IPluginInfo.PluginFullName"/>).</param>
        /// <returns>
        /// The result with a potential non null <see cref="IYodiiEngineResult.StaticFailureResult"/> but always an 
        /// available <see cref="IYodiiEngineStaticOnlyResult.StaticSolvedConfiguration"/>.
        /// </returns>
        public IYodiiEngineStaticOnlyResult StaticResolutionOnly( bool revertServices, bool revertPlugins )
        {
            var r = ConfigurationSolver.CreateAndApplyStaticResolution( this, _manager.FinalConfiguration, _discoveredInfo, revertServices, revertPlugins, createStaticSolvedConfigOnSuccess:true );
            Debug.Assert( r.Item1 != null, "Either an error or a successful static resolution." );
            return r.Item1;
        }

        public IYodiiEngineResult Start( bool revertServices, bool revertPlugins, IEnumerable<YodiiCommand> persistedCommands = null )
        {
            if( !IsRunning )
            {
                _yodiiCommands.Clear();
                if( persistedCommands != null ) _yodiiCommands.AddRange( persistedCommands );
                var r = ConfigurationSolver.CreateAndApplyStaticResolution( this, _manager.FinalConfiguration, _discoveredInfo, revertServices, revertPlugins, false );
                if( r.Item1 != null )
                {
                    Debug.Assert( !r.Item1.Success, "Not null means necessarily an error." );
                    Debug.Assert( r.Item1.Engine == this );
                    return r.Item1;
                }
                return DoDynamicResolution( r.Item2, null, null );
            }
            return _successResult;
        }

        public IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo info )
        {
            if( info == null ) throw new ArgumentNullException( "info" );
            if( info == _discoveredInfo ) return _successResult;

            if( IsRunning )
            {
                var r = ConfigurationSolver.CreateAndApplyStaticResolution( this, _manager.FinalConfiguration, info, false, false, false );
                if( r.Item1 != null )
                {
                    Debug.Assert( !r.Item1.Success, "Not null means necessarily an error." );
                    Debug.Assert( r.Item1.Engine == this );
                    return r.Item1;
                }
                return DoDynamicResolution( r.Item2, null, null, () => DiscoveredInfo = info );
            }
            else DiscoveredInfo = info;
            return _successResult;
        }

        /// <summary>
        /// YodiiCommands are exposed by LiveInfo, not by the IYodiiEngine itself.
        /// </summary>
        internal IObservableReadOnlyList<YodiiCommand> YodiiCommands
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
