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
        /// Gets an immutable list of dynamic items information (the union of <see cref="Plugins"/> and <see cref="Services"/>).
        /// </summary>
        IReadOnlyList<IDynamicSolvedYodiiItem> YodiiItems { get; }

        /// <summary>
        /// Finds a plugin or a service by its full name.
        /// </summary>
        /// <param name="fullName">Service's or Plugin's full name.</param>
        /// <returns>Solved dynamic item information.</returns>
        IDynamicSolvedYodiiItem FindItem( string fullName );

        /// <summary>
        /// Finds a service by its full name.
        /// </summary>
        /// <param name="serviceFullName">Service's full name.</param>
        /// <returns>Solved dynamic service.</returns>
        IDynamicSolvedService FindService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Solved dynamic plugin.</returns>
        IDynamicSolvedPlugin FindPlugin( string pluginFullName );
    }
}
