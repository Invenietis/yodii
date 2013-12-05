using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Failure result during static resolution.
    /// </summary>
    public interface IStaticFailureResult
    {
        /// <summary>
        /// Solved static configuration.
        /// </summary>
        IStaticSolvedConfiguration StaticSolvedConfiguration { get; }

        /// <summary>
        /// Plugins that blocked the static resolution.
        /// </summary>
        IReadOnlyList<IStaticSolvedPlugin> BlockingPlugins { get; }

        /// <summary>
        /// Services that blocked the static resolution.
        /// </summary>
        IReadOnlyList<IStaticSolvedService> BlockingServices { get; }
    }
}
