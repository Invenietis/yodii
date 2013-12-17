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
        public readonly string CallerKey;

        /// <summary>
        /// True if it is a command to start a plugin/service, false to stop a plugin/service.
        /// </summary>
        public readonly bool Start;

        /// <summary>
        /// If acting on a plugin's status: plugin full name.
        /// <seealso cref="YodiiCommand.ServiceFullName"/>
        /// </summary>
        public readonly string PluginFullName;

        /// <summary>
        /// When acting on a plugin, range of impact on the plugin's dependencies.
        /// </summary>
        public readonly StartDependencyImpact Impact;

        /// <summary>
        /// If acting on a service: service full name.
        /// <seealso cref="YodiiCommand.PluginFullName"/>
        /// </summary>
        public readonly string ServiceFullName;

        YodiiCommand( string callerKey, bool start, StartDependencyImpact impact )
        {
            if( callerKey == null ) throw new ArgumentNullException( "callerKey" );
            if( !start && impact != StartDependencyImpact.Unknown ) throw new ArgumentException( "Impact must be None when stopping a plugin or a service.", "impact" );
            CallerKey = callerKey;
            Start = start;
            Impact = impact;
        }

        /// <summary>
        /// Creates a new YodiiCommand, to act on a plugin's status.
        /// </summary>
        /// <param name="callerKey">Calling object identifier.</param>
        /// <param name="start">True to start the plugin, false to stop the plugin.</param>
        /// <param name="impact">Range of impact on the plugin's dependencies.</param>
        /// <param name="serviceOrPluginId">Plugin ID to act on.</param>
        public YodiiCommand( string callerKey, string serviceOrPluginId, bool start, StartDependencyImpact impact = StartDependencyImpact.Unknown, bool isPlugin = false )
            : this( callerKey, start, impact )
        {
            if( string.IsNullOrWhiteSpace( serviceOrPluginId ) ) throw new ArgumentException( "Must be not null nor empty nor white space.", "serviceOrPluginId" );
            if( isPlugin )
            {
                PluginFullName = serviceOrPluginId;
            }
            else
            {
                ServiceFullName = serviceOrPluginId;
            }
        }

    }
}
