using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Engine;
using Yodii.Model;
using Yodii.Discoverer;
using System.IO;
using CK.Core;
using System.Collections.ObjectModel;

namespace Yodii.DemoApp
{
    public class DemoAppManager : IYodiiEngineHost
    {
        readonly IYodiiEngine _engine;
        readonly StandardDiscoverer _discoverer;
        readonly ObservableCollection<IPluginInfo> _runningPlugins;

        internal DemoAppManager()
        {
            _engine = new YodiiEngine( this );
            _discoverer = new StandardDiscoverer();
            _runningPlugins = new ObservableCollection<IPluginInfo>();

            RetrieveGraph();
        }

        internal void RetrieveGraph()
        {
            _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.DemoApp.dll" ) );
            _engine.SetDiscoveredInfo( _discoverer.GetDiscoveredInfo() );
            SetAllToRunning();
        }

        /// <summary>
        /// Sets all items status to ConfigurationStatus.Running.
        /// </summary>
        internal void SetAllToRunning()
        {
            var items = _engine.Configuration.Layers.SelectMany( i => i.Items );
            foreach( var i in items )
            {
                i.SetStatus( ConfigurationStatus.Running );
            }
        }

        public IEnumerable<Tuple<IPluginInfo, Exception>> Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart )
        {
            List<Tuple<IPluginInfo, Exception>> exceptionList = new List<Tuple<IPluginInfo, Exception>>();

            Console.WriteLine( "Disabling:" );
            foreach( var plugin in toDisable )
            {
                Console.WriteLine( String.Format( "- {0}", plugin.PluginFullName ) );
                if( _runningPlugins.Contains( plugin ) )
                {
                    _runningPlugins.Remove( plugin );
                }
            }

            Console.WriteLine( "Stopping:" );
            foreach( var plugin in toStop )
            {
                Console.WriteLine( String.Format( "- {0}", plugin.PluginFullName ) );
                if( _runningPlugins.Contains( plugin ) )
                {
                    _runningPlugins.Remove( plugin );
                }
            }

            Console.WriteLine( "Starting:" );
            foreach( var plugin in toStart )
            {
                Console.WriteLine( String.Format( "- {0}", plugin.PluginFullName ) );
                if( !_runningPlugins.Contains( plugin ) )
                {
                    _runningPlugins.Add( plugin );
                }
            }

            return exceptionList;
        }

        internal IYodiiEngine Engine
        {
            get { return _engine; }
        }

        internal ObservableCollection<IPluginInfo> RunningPlugins
        {
            get { return _runningPlugins; }
        }
    }
}
