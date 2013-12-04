using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public enum PluginRunningStatusReason
    {
        None = 0,

        StartedByConfig,

        StoppedByConfig,

        StartedByRunningService,

        StoppedByStoppedService,

        StartedByCommand,
        StoppedByCommand,
        StoppedByRunningSibling,
        StoppedByStoppedReference,
        StoppedByFinalDecision
    }
}
