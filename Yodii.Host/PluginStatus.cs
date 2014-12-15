using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    /// <summary>
    /// Defines the status of a plugin with its <see cref="Stopping"/> and <see cref="Starting"/> transitions.
    /// </summary>
    public enum PluginStatus
    {
        /// <summary>
        /// Plugin is not instanciated.
        /// </summary>
        Null = 0,

        /// <summary>
        /// Plugin is stopped.
        /// </summary>
        Stopped = 1,
        
        /// <summary>
        /// Plugin is stopping: <see cref="IYodiiPlugin.PreStop"/> has been called
        ///  but not <see cref="IYodiiPlugin.Stop"/> yet.
        /// </summary>
        Stopping = 2,

        /// <summary>
        /// Plugin is starting: <see cref="IYodiiPlugin.PreStart"/> has been called 
        /// but not <see cref="IYodiiPlugin.Start"/> yet.
        /// </summary>
        Starting = 3,
        
        /// <summary>
        /// Plugin is running.
        /// </summary>
        Started = 4
    }
}
