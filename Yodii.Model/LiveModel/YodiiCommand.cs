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
    /// Command sent to the engine to start or stop plugins or services, with details on how it should proceed.
    /// </summary>
    public class YodiiCommand
    {
        /// <summary>
        /// Identifies the object that requested the command.
        /// <seealso cref="ILiveInfo.RevokeCaller(string)"/>
        /// </summary>
        public string CallerKey { get { return _callerKey; } }
        private readonly string _callerKey;

        /// <summary>
        /// True if it is a command to start a plugin/service, false to stop a plugin/service.
        /// </summary>
        public bool Start { get { return _start; } }
        private readonly bool _start;

        /// <summary>
        /// If acting on a plugin's status: plugin full name.
        /// <seealso cref="YodiiCommand.ServiceFullName"/>
        /// </summary>
        public string PluginFullName { get { return _pluginFullName; } }
        private readonly string _pluginFullName;

        /// <summary>
        /// When acting on a plugin, range of impact on the plugin's dependencies.
        /// </summary>
        public StartDependencyImpact Impact { get { return _impact; } }
        private readonly StartDependencyImpact _impact;

        /// <summary>
        /// If acting on a service: service full name.
        /// <seealso cref="YodiiCommand.PluginFullName"/>
        /// </summary>
        public string ServiceFullName { get { return _serviceFullName; } }
        private readonly string _serviceFullName;

        YodiiCommand( string callerKey, bool start, StartDependencyImpact impact )
        {
            if( callerKey == null ) throw new ArgumentNullException( "callerKey" );
            if( !start && impact != StartDependencyImpact.Unknown ) throw new ArgumentException( "Impact must be None when stopping a plugin or a service.", "impact" );
            _callerKey = callerKey;
            _start = start;
            _impact = impact;
        }

        /// <summary>
        /// Creates a new YodiiCommand, to act on a plugin's status.
        /// </summary>
        /// <param name="callerKey">Calling object identifier.</param>
        /// <param name="start">True to start the plugin, false to stop the plugin.</param>
        /// <param name="impact">Range of impact on the plugin's dependencies.</param>
        /// <param name="serviceOrPluginFullName">Plugin ID to act on.</param>
        /// <param name="isPlugin">Set to true if command is for a plugin. False for a service.</param>
        public YodiiCommand( string callerKey, string serviceOrPluginFullName, bool start, StartDependencyImpact impact = StartDependencyImpact.Unknown, bool isPlugin = false )
            : this( callerKey, start, impact )
        {
            if( string.IsNullOrWhiteSpace( serviceOrPluginFullName ) ) throw new ArgumentException( "Must be not null nor empty nor white space.", "serviceOrPluginFullName" );
            if( isPlugin )
            {
                _pluginFullName = serviceOrPluginFullName;
            }
            else
            {
                _serviceFullName = serviceOrPluginFullName;
            }
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
