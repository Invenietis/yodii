using System;
using System.Collections.Generic;
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

        public YodiiEngine( IYodiiEngineHost host )
        {
            if( host == null ) throw new ArgumentNullException( "host" );
            _host = host;
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
            _manager = new ConfigurationManager( this );
            _yodiiCommands = new List<YodiiCommand>();
        }

        void _manager_ConfigurationChanging( object sender, ConfigurationChangingEventArgs e )
        {
            // Prechanging
            ConfigurationSolver s = new ConfigurationSolver();
            IStaticFailureResult sr = s.StaticResolution( e.FinalConfiguration, _discoveredInfo );
            if( sr != null )
            {
                e.CancelByStaticResolution( sr );
                return;
            }

            // Postchanging
            var toDo = s.DynamicResolution( _yodiiCommands );
            var errors = _host.Apply( toDo.Item1, toDo.Item2, toDo.Item3 );
            if( errors != null && errors.Any() )
            {
                IDynamicFailureResult dr = s.CreateDynamicFailureResult( errors );
                e.CancelByDynamicStep( dr );
                return;
            }
            _currentSolver = s;

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
