using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Solved configuration after dynamic resolution.
    /// </summary>
    public interface IDynamicSolvedConfiguration
    {
        /// <summary>
        /// Solved dynamic plugins.
        /// </summary>
        IReadOnlyList<IDynamicSolvedPlugin> Plugins { get; }

        /// <summary>
        /// Solved dynamic services.
        /// </summary>
        IReadOnlyList<IDynamicSolvedService> Services { get; }

        /// <summary>
        /// Finds a service by its full name.
        /// </summary>
        /// <param name="fullName">Service's full name.</param>
        /// <returns>Solved dynamic service.</returns>
        IDynamicSolvedService FindService( string fullName );

        /// <summary>
        /// Finds a plugin by its GUID.
        /// </summary>
        /// <param name="pluginId">Plugin GUID</param>
        /// <returns>Solved dynamic plugin.</returns>
        IDynamicSolvedPlugin FindPlugin( Guid pluginId );
    }
}
