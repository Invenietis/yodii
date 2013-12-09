using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    /// <summary>
    /// Yodii engine result, containing various data regarding the engine operation.
    /// </summary>
    public interface IYodiiEngineResult
    {
        /// <summary>
        /// Whether the operation was considered a success.
        /// </summary>
        bool Success { get; }

        /// <summary>
        /// Details of errors encountered during resolution of configuration.
        /// </summary>
        IConfigurationFailureResult ConfigurationFailureResult { get; }

        /// <summary>
        /// Details of errors encountered during static resolution.
        /// </summary>
        IStaticFailureResult StaticFailureResult { get; }

        /// <summary>
        /// Details of errors encountered during host startup and plugin startup/shutdown.
        /// </summary>
        IDynamicFailureResult HostFailureResult { get; }

        /// <summary>
        /// List of plugins causing the failure.
        /// </summary>
        IReadOnlyList<IPluginInfo> PluginCulprits { get; }
        
        /// <summary>
        /// List of services causing the failure.
        /// </summary>
        IReadOnlyList<IServiceInfo> ServiceCulprits { get; }
    }
}
