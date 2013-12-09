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
        /// Object that requested the command.
        /// <seealso cref="ILiveInfo.RevokeCaller(object)"/>
        /// </summary>
        public readonly object Caller;

        /// <summary>
        /// True if it is a command to start a plugin/service, false to stop a plugin/service.
        /// </summary>
        public readonly bool Start;

        /// <summary>
        /// If acting on a plugin's status: plugin ID.
        /// <seealso cref="YodiiCommand.FullName"/>
        /// </summary>
        public readonly Guid PluginId;

        /// <summary>
        /// When acting on a plugin, range of impact on the plugin's dependencies.
        /// </summary>
        public readonly StartDependencyImpact Impact;

        /// <summary>
        /// If acting on a service: service full name.
        /// <seealso cref="YodiiCommand.PluginId"/>
        /// </summary>
        public readonly string FullName;

        private YodiiCommand( object caller, bool start )
        {
            Caller = caller;
            Start = start;
        }

        /// <summary>
        /// Creates a new YodiiCommand, to act on a plugin's status.
        /// </summary>
        /// <param name="caller">Calling object.</param>
        /// <param name="start">True to start the plugin, false to stop the plugin.</param>
        /// <param name="impact">Range of impact on the plugin's dependencies.</param>
        /// <param name="pluginId">Plugin ID to act on.</param>
        public YodiiCommand( object caller, bool start, StartDependencyImpact impact, Guid pluginId )
            : this( caller, start )
        {
            Debug.Assert( pluginId != null );
            PluginId = pluginId;
            Impact = impact;
        }

        /// <summary>
        /// Creates a new YodiiCommand, to act on a service's status.
        /// </summary>
        /// <param name="caller">Calling object.</param>
        /// <param name="start">True to start the service, false to stop the service.</param>
        /// <param name="fullName">Service full name to act on.</param>
        public YodiiCommand( object caller, bool start, string fullName )
            : this( caller, start )
        {
            Debug.Assert( string.IsNullOrEmpty( fullName ) != true );
            FullName = fullName;
        }
    }
}
