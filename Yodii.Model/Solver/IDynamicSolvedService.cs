using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Solved dynamic service data.
    /// </summary>
    public interface IDynamicSolvedService : IDynamicSolvedYodiiItem
    {
        /// <summary>
        /// Gets the service information.
        /// </summary>
        IServiceInfo ServiceInfo { get; }

    }
}
