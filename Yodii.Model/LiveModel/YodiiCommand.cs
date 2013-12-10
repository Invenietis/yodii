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
        public readonly string CallerKey;
        public readonly Guid PluginId;
        public readonly string ServiceFullName;
        public readonly bool Start;
        public readonly StartDependencyImpact Impact;

        YodiiCommand( string callerKey, bool start, StartDependencyImpact impact )
        {
            if( callerKey != null ) throw new ArgumentNullException( "callerKey" );
            if( !start && impact != StartDependencyImpact.None ) throw new ArgumentException( "Impact must be None when stopping a plugin or a service.", "impact" );
            CallerKey = callerKey;
            Start = start;
            Impact = impact;
        }

        public YodiiCommand( string callerKey, Guid pluginId, bool start, StartDependencyImpact impact = StartDependencyImpact.None )
            : this( callerKey, start, impact )
        {
            PluginId = pluginId;
        }

        public YodiiCommand( string callerKey, string serviceFullName, bool start, StartDependencyImpact impact = StartDependencyImpact.None )
            : this( callerKey, start, impact )
        {
            if( !string.IsNullOrWhiteSpace( serviceFullName ) ) throw new ArgumentException( "Must be not null nor empty nor white space.", "serviceFullName" );
            ServiceFullName = serviceFullName;
        }
    }
}
