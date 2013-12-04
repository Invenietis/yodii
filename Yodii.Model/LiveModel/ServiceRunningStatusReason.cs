using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public enum ServiceRunningStatusReason
    {
        None = 0,

        StartedByConfig,

        StoppedByConfig,

        StoppedByGeneralization,

        StoppedByCommand,

        StoppedByPluginStopped,
        StartedBySpecialization,
        StoppedBySiblingRunningService,
        StartedByCommand,
        StartedByPlugin,

        StartedByOptionalReference,
        StartedByOptionalTryStartReference,
        StartedByRunnableReference,
        StartedByRunnableTryStartReference,
        StartedByRunningReference,

        StoppedByOptionalReference,
        StoppedByOptionalTryStartReference,
        StoppedByRunnableReference,
        StoppedByRunnableTryStartReference,
        StoppedByFinalDecision,
    }
}
