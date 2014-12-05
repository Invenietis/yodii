using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    enum PluginStatus
    {
        Disabled = ServiceStatus.Disabled,

        Stopped = ServiceStatus.Stopped,
        
        Stopping = ServiceStatus.Stopping,
        
        Starting = ServiceStatus.Starting,
        
        Started = ServiceStatus.Started
    }
}
