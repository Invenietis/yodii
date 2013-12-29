using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Immutable object that describes the start or stop of a plugin or a service.
    /// </summary>
    public class YodiiCommand
    {
        readonly string _callerKey;
        readonly string _fullName;
        readonly StartDependencyImpact _impact;
        readonly bool _start;
        readonly bool _isPlugin;

        /// <summary>
        /// True if it is a command to start a plugin/service, false to stop a plugin/service.
        /// </summary>
        public bool Start { get { return _start; } }

        /// <summary>
        /// Gets the impact on the dependencies.
        /// Always <see cref="StartDependencyImpact.Unknown"/> when <see cref="Start"/> is false.
        /// </summary>
        public StartDependencyImpact Impact { get { return _impact; } }

        /// <summary>
        /// Gets whether this command applies to a plugin.
        /// </summary>
        public bool IsPlugin { get { return _isPlugin; } }

        /// <summary>
        /// Gets the plugin full name that must be started or stopped. 
        /// Null if this command targets a service.
        /// </summary>
        public string PluginFullName { get { return _isPlugin ? _fullName : null; } }

        /// <summary>
        /// Gets the service full name that must be started or stopped. 
        /// Null if this command targets a plugin.
        /// </summary>
        public string ServiceFullName { get { return _isPlugin ? null : _fullName; } }

        /// <summary>
        /// Identifies the object that emitted the command.
        /// Callers can be revoked thanks to <see cref="ILiveInfo.RevokeCaller(string)"/>.
        /// </summary>
        public string CallerKey { get { return _callerKey; } }

        /// <summary>
        /// Initializes a new YodiiCommand.
        /// </summary>
        /// <param name="start">True to start the plugin or the service, false to stop it.</param>
        /// <param name="fullName">The plugin or service name.</param>
        /// <param name="isPlugin">True if command is for a plugin, false for a service.</param>
        /// <param name="impact">Dependency impact when starting. Must be <see cref="StartDependencyImpact.Unknown"/> when <paramref name="start"/> is false.</param>
        /// <param name="callerKey">Calling object identifier. Can be null: it is normalized to the empty string.</param>
        public YodiiCommand( bool start, string fullName, bool isPlugin, StartDependencyImpact impact, string callerKey )
        {
            if( !start && impact != StartDependencyImpact.Unknown ) throw new ArgumentException( "Impact must be None when stopping a plugin or a service.", "impact" );
            if( String.IsNullOrEmpty( fullName ) ) throw new ArgumentException( "A service or plugin name can not be null or empty.", "fullName" );
            _callerKey = callerKey ?? String.Empty;
            _start = start;
            _impact = impact;
            _fullName = fullName;
            _isPlugin = isPlugin;
        }

        /// <summary>
        /// Describes the command.
        /// </summary>
        /// <returns>Explicit description of the command.</returns>
        public override string ToString()
        {
            if( CallerKey.Length > 0 )
                return String.Format( "{0} {1}, impact={2}, by {3}.", Start ? "Start" : "Stop", PluginFullName ?? ServiceFullName, Impact, CallerKey );
            return String.Format( "{0} {1}, impact={2}.", Start ? "Start" : "Stop", PluginFullName ?? ServiceFullName, Impact );
        }
    }
}
