using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Describes service information resulting from static resolution.
    /// </summary>
    public interface IStaticSolvedService : IStaticSolvedYodiiItem
    {
        /// <summary>
        /// Service information.
        /// </summary>
        IServiceInfo ServiceInfo { get; }
    }
}
