using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Model
{
    public class YodiiCommand
    {
        public readonly object Caller;
        public readonly bool Start;
        public readonly Guid PluginId;
        public readonly StartDependencyImpact Impact;
        public readonly string FullName;

        private YodiiCommand( object caller, bool start )
        {
            Caller = caller;
            Start = start;
        }

        public YodiiCommand( object caller, bool start, StartDependencyImpact impact, Guid pluginId )
            : this( caller, start )
        {
            Debug.Assert( pluginId != null );
            PluginId = pluginId;
            Impact = impact;
        }

        public YodiiCommand( object caller, bool start, string fullName )
            : this( caller, start )
        {
            Debug.Assert( string.IsNullOrEmpty( fullName ) != true );
            FullName = fullName;
        }
    }
}
