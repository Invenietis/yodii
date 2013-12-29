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
        /// Gets an immutable list of static solved plugins.
        /// </summary>
        IReadOnlyList<IStaticSolvedPlugin> Plugins { get; }

        /// <summary>
        /// Gets an immutable list of static solved services.
        /// </summary>
        IReadOnlyList<IStaticSolvedService> Services { get; }

        /// <summary>
        /// Gets an immutable list of static items information (the union of <see cref="Plugins"/> and <see cref="Services"/>).
        /// </summary>
        IReadOnlyList<IStaticSolvedYodiiItem> YodiiItems { get; }

        /// <summary>
        /// Finds a plugin or a service by its full name.
        /// </summary>
        /// <param name="fullName">Service's or Plugin's full name.</param>
        /// <returns>Static solved Yodii item information.</returns>
        IStaticSolvedYodiiItem FindItem( string fullName );

        /// <summary>
        /// Finds a service by its full name.
        /// </summary>
        /// <param name="serviceFullName">Service's full name.</param>
        /// <returns>Static solved service.</returns>
        IStaticSolvedService FindService( string serviceFullName );

        /// <summary>
        /// Finds a plugin by its full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Static solved plugin.</returns>
        IStaticSolvedPlugin FindPlugin( string pluginFullName );
    }
}
