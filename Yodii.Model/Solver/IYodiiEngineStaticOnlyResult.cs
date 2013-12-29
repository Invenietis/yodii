using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Exposes information specific to <see cref="IYodiiEngine.StaticResolutionOnly"/> method.
    /// </summary>
    public interface IYodiiEngineStaticOnlyResult : IYodiiEngineResult
    {
        /// <summary>
        /// Always gives access to the solved configuration, be the result on success or on failure.
        /// </summary>
        IStaticSolvedConfiguration StaticSolvedConfiguration { get; }
    }
}
