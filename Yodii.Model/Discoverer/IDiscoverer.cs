using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Abstraction of the discoverer.
    /// </summary>
    public interface IDiscoverer
    {
        /// <summary>
        /// Reads an assembly from a path.
        /// </summary>
        /// <param name="path">Path of the assembly to discover.</param>
        /// <returns>The extracted information.</returns>
        IAssemblyInfo ReadAssembly( string path );

        /// <summary>
        /// Obtains a snapshot (immutable) of the discovered informations: contains all the currently discovered assemblies.
        /// </summary>
        /// <param name="withAssembliesOnError">True to retrieve the assemblies on error.</param>
        /// <returns>Snapshot of the information.</returns>
        IDiscoveredInfo GetDiscoveredInfo( bool withAssembliesOnError = false );
    }
}
