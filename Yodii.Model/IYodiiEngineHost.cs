using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IYodiiEngineHost
    {
        IEnumerable<Tuple<IPluginInfo, Exception>> Apply( IEnumerable<IPluginInfo> toDisable, IEnumerable<IPluginInfo> toStop, IEnumerable<IPluginInfo> toStart ); 

    }
}
