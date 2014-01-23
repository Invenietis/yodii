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
        /// Solved static configuration. Never null.
        /// </summary>
        IStaticSolvedConfiguration StaticSolvedConfiguration { get; }

        /// <summary>
        /// Plugins or Services that blocked the static resolution.
        /// Never null (but can be empty).
        /// </summary>
        IReadOnlyList<IStaticSolvedYodiiItem> BlockingItems { get; }

        /// <summary>
        /// Services that blocked the static resolution.
        /// Never null (but can be empty).
        /// </summary>
        IReadOnlyList<IStaticSolvedService> BlockingServices { get; }

        /// <summary>
        /// Plugins that blocked the static resolution.
        /// Never null (but can be empty).
        /// </summary>
        IReadOnlyList<IStaticSolvedPlugin> BlockingPlugins { get; }
    }
}
