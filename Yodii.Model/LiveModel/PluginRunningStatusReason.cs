using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Description of the reason behind a running/stopped plugin.
    /// </summary>
    public enum PluginRunningStatusReason
    {
        /// <summary>
        /// No reason. Just because!
        /// </summary>
        None = 0,

        /// <summary>
        /// Configuration required this plugin to be started.
        /// </summary>
        StartedByConfig,

        /// <summary>
        /// Configuration required this plugin to be stopped.
        /// </summary>
        StoppedByConfig,

        /// <summary>
        /// A running implemented service required this plugin to start.
        /// </summary>
        StartedByRunningService,

        /// <summary>
        /// A stopping implemented service required this plugin to stop.
        /// </summary>
        StoppedByStoppedService,

        /// <summary>
        /// Plugin started by command.
        /// </summary>
        StartedByCommand,

        /// <summary>
        /// Plugin stopped by command.
        /// </summary>
        StoppedByCommand,

        /// <summary>
        /// Plugin stopped because one of its siblings was started.
        /// </summary>
        StoppedByRunningSibling,

        /// <summary>
        /// Plugin stopped because one if the services it references was stopped.
        /// </summary>
        StoppedByStoppedReference,

        /// <summary>
        /// Plugin stopped during the end of resolution.
        /// </summary>
        StoppedByFinalDecision
    }
}
