using System;
using System.Collections.Generic;
using CK.Core;

namespace Yodii.Model
{
    /// <summary>
    /// Assembly information for Yodii plugins and services.
    /// </summary>
    public interface IAssemblyInfo 
    {
        /// <summary>
        /// Gets the assembly location.
        /// </summary>
        Uri AssemblyLocation { get; }

        /// <summary>
        /// Gets the plugins located in this assembly.
        /// </summary>
        IReadOnlyList<IPluginInfo> Plugins { get; }
        
        /// <summary>
        /// Gets the services located in this assembly.
        /// </summary>
        IReadOnlyList<IServiceInfo> Services { get; }
    }
}
