using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Solved configuration after static resolution.
    /// </summary>
    public interface IStaticSolvedConfiguration
    {
        /// <summary>
        /// List of static solved plugins.
        /// </summary>
        IReadOnlyList<IStaticSolvedPlugin> Plugins { get; }

        /// <summary>
        /// List of static solved services.
        /// </summary>
        IReadOnlyList<IStaticSolvedService> Services { get; }

        /// <summary>
        /// Finds a service by its full name.
        /// </summary>
        /// <param name="fullName">Service's full name.</param>
        /// <returns>Static solved service.</returns>
        IStaticSolvedService FindService( string fullName );

        /// <summary>
        /// Finds a plugin by its GUID.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Static solved plugin.</returns>
        IStaticSolvedPlugin FindPlugin( string pluginFullName );
    }
}
