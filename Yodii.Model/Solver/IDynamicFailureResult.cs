using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Failure result during application of dynamic resolution on <see cref="IYodiiEngineHost"/>.
    /// <seealso cref="IYodiiEngineHost.Apply( IEnumerable{IPluginInfo}, IEnumerable{IPluginInfo}, IEnumerable{IPluginInfo})"/>
    /// </summary>
    public interface IDynamicFailureResult
    {
        /// <summary>
        /// Dynamic resolution configuration that was attempted.
        /// </summary>
        IDynamicSolvedConfiguration SolvedConfiguration { get; }

        /// <summary>
        /// List of plugin runtime errors causing this failure.
        /// </summary>
        IReadOnlyList<PluginRuntimeError> ErrorPlugins { get; }
    }
}
