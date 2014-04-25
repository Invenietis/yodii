using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IPluginDiscoverer
    {
        /// <summary>
        /// Fires at the beginning of a discovery process.
        /// </summary>
        event EventHandler DiscoverBegin;

        /// <summary>
        /// Fires at the end of the discovery process: all plugins information is available
        /// and up to date.
        /// </summary>
        //event EventHandler<DiscoverDoneEventArgs> DiscoverDone;

        /// <summary>
        /// Contains all the <see cref="IAssemblyInfo"/> that have been processed.
        /// They may contain an error or no plugins at all.
        /// </summary>
        IReadOnlyCollection<IAssemblyInfo> AllAssemblies { get; }

        /// <summary>
        /// Contains all the <see cref="IAssemblyInfo"/> that have been succesfully discovered 
        /// and have at least one plugin or one service defined in it.
        /// </summary>
        IReadOnlyCollection<IAssemblyInfo> PluginOrServiceAssemblies { get; }

        /// <summary>
        /// Contains all the <see cref="IPluginInfo"/> that have been succesfully discovered with the best
        /// available version.
        /// </summary>
        IReadOnlyCollection<IPluginInfo> Plugins { get; }

        /// <summary>
        /// Contains all the <see cref="IServiceInfo"/> that have been succesfully discovered
        /// with their implementations.
        /// </summary>
        IReadOnlyCollection<IServiceInfo> Services { get; }

       
        /// <summary>
        /// Gets <see cref="IPluginInfo"/> best version with the given plugin identifier.
        /// </summary>
        /// <param name="pluginId"></param>
        /// <returns></returns>
        IPluginInfo FindPlugin( string pluginFullName );

        /// <summary>
        /// Gets the <see cref="IServiceInfo"/> associated to the given assembly qualified name.
        /// </summary>
        /// <param name="assemblyQualifiedName"></param>
        /// <returns></returns>
        IServiceInfo FindService( string serviceFullName );

        /// <summary>
        /// Gets the number of discover previously done.
        /// </summary>
        int CurrentVersion { get; }

        /// <summary>
        /// Start the discover in a given <see cref="DirectoryInfo"/>.
        /// </summary>
        /// <param name="dir">Directory that we have to look into.</param>
        /// <param name="recurse">Sets if the discover is recursive of not.</param>
        //void Discover( DirectoryInfo dir, bool recurse );

        /// <summary>
        /// Discover only one file.
        /// </summary>
        /// <param name="file">An exisiting file (a dll).</param>
        //void Discover( FileInfo file );
    }
}
