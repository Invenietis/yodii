using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class DynamicSolverResult
    {
        public readonly IReadOnlyList<IPluginInfo> Disabled;
        public readonly IReadOnlyList<IPluginInfo> Stopped;
        public readonly IReadOnlyList<IPluginInfo> Running;
        public readonly IReadOnlyList<YodiiCommand> Commands;

        public DynamicSolverResult( IReadOnlyList<IPluginInfo> disabled, IReadOnlyList<IPluginInfo> stopped, IReadOnlyList<IPluginInfo> running, IReadOnlyList<YodiiCommand> commands )
        {
            Disabled = disabled;
            Stopped = stopped;
            Running = running;
            Commands = commands;
        }
    }
}
