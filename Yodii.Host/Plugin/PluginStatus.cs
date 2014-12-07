using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    enum PluginStatus
    {
        Disabled = 0,

        Stopped = 1,
        
        Stopping = 2,
        
        Starting = 3,
        
        Started = 4
    }
}
