using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class YodiiCommand
    {
        public readonly object Caller;
        public readonly bool Start;
        public readonly Guid PluginId;
        public readonly StartDependencyImpact Impact;
        public readonly string FullName;

        private YodiiCommand( object caller, bool start, StartDependencyImpact impact )
        {
            Caller = caller;
            Start = start;
            Impact = impact;
        }

        internal YodiiCommand( object caller, bool start, StartDependencyImpact impact, Guid pluginId )
            : this( caller, start, impact )
        {
            PluginId = pluginId;
        }

        internal YodiiCommand( object caller, bool start, StartDependencyImpact impact, string fullName )
            : this( caller, start, impact )
        {
            FullName = fullName;
        }
    }
}
