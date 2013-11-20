using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public struct PluginRuntimeError
    {
        public readonly IDynamicSolvedPlugin Plugin;

        public readonly Exception Error;

        public PluginRuntimeError( IDynamicSolvedPlugin plugin, Exception error )
        {
            Plugin = plugin;
            Error = error;
        }
    }
}
