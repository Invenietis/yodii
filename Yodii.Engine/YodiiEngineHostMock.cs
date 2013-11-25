using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class YodiiEngineHostMock : IYodiiEngineHost
    {
        internal YodiiEngineHostMock()
        {

        }

        public IEnumerable<Tuple<IPluginInfo, Exception>> Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart )
        {
            Debug.Assert( toDisable.Any() || toStop.Any() || toStart.Any() );
            List<Tuple<IPluginInfo, Exception>> pluginErrors = new List<Tuple<IPluginInfo, Exception>>();

            foreach(IPluginInfo pluginToDisable in toDisable)
            {
                if( pluginToDisable.PluginFullName.Contains("buggy") )
                {
                    pluginErrors.Add( new Tuple<IPluginInfo, Exception>( pluginToDisable, new Exception( "HostError" ) ) );
                }
            }
            foreach ( IPluginInfo pluginToStop in toStop )
            {
                if ( pluginToStop.PluginFullName.Contains( "buggy" ) )
                {
                    pluginErrors.Add( new Tuple<IPluginInfo, Exception>( pluginToStop, new Exception( "HostError" ) ) );
                }
            }
            foreach ( IPluginInfo pluginToStart in toStart )
            {
                if ( pluginToStart.PluginFullName.Contains( "buggy" ) )
                {
                    pluginErrors.Add( new Tuple<IPluginInfo, Exception>( pluginToStart, new Exception( "HostError" ) ) );
                }
            }
            if ( pluginErrors.Any() ) return pluginErrors;
            return null;
        }
    }
}
