using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface IAssemblyInfo
    {
        /// <summary>
        /// 
        /// </summary>
        string AssemblyFileName { get; }

        /// <summary>
        /// Gets that the assembly contains plugins or services.
        /// </summary>
        bool HasPluginsOrServices { get; }

        /// <summary>
        /// Gets the collections of plugins contained into the assembly.
        /// </summary>
        IReadOnlyList<IPluginInfo> Plugins { get; }

        /// <summary>
        /// Gets the collections of services contained into the assembly.
        /// </summary>
        IReadOnlyList<IServiceInfo> Services { get; }
    }
}
