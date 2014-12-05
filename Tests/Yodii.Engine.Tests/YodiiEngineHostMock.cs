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
            IEnumerable<IPluginInfo> toCheck = toDisable.Concat( toStop ).Concat( toStart );

            foreach( IPluginInfo plugin in toCheck )
            {
                if( plugin.PluginFullName.Contains( "buggy" ) )
                {
                    pluginErrors.Add( new Tuple<IPluginInfo, Exception>( plugin, new Exception( "HostError" ) ) );
                }
            }

            if( pluginErrors.Any() ) return pluginErrors;
            return null;
        }
    }
}
