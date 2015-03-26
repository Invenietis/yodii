#if DEBUG
using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Mocks
{
    /// <summary>
    /// Helper utility used to start and handle a Yodii Engine at design time.
    /// </summary>
    static class MockEngineHelper
    {
        internal static YodiiEngine Engine { get; private set; }
        internal static StandardDiscoverer Discoverer { get; private set; }
        internal static YodiiHost Host { get; private set; }

        internal static void StartDesignTimeYodiiEngine()
        {
            Host = new YodiiHost();
            Engine = new YodiiEngine( Host );

            // At design time, assemblies are copied somewhere in AppData\Local\Microsoft\VisualStudio\12.0\Designer\ShadowCache.
            // This makes the resolution somewhat difficult, as each assembly is copied in a subfolder without its dependencies.
            string[] binPaths = GetBinPaths();

            Discoverer = new StandardDiscoverer( binPaths );

            foreach( var ap in GetYodiiAssemblyPaths() )
            {
                Discoverer.ReadAssembly( ap );
            }

            IDiscoveredInfo info = Discoverer.GetDiscoveredInfo();
            Engine.Configuration.SetDiscoveredInfo( info );

            IYodiiEngineResult result = Engine.StartEngine();
            if( result.Success == false )
            {
                MessageBox.Show( "Couldn't start engine." );
            }
            else
            {
                //MessageBox.Show( "Started design-time engine.\nPlugins:" + String.Join("\n", Engine.LiveInfo.Plugins.Select( p => p.FullName )) );
            }
        }

        internal static string GetAssemblyDirectory( Assembly a )
        {
            string codeBase = a.CodeBase;
            UriBuilder uri = new UriBuilder( codeBase );
            string path = Uri.UnescapeDataString( uri.Path );
            return Path.GetDirectoryName( path );
        }

        internal static string GetAssemblyPath( Assembly a )
        {
            string codeBase = a.CodeBase;
            UriBuilder uri = new UriBuilder( codeBase );
            string path = Uri.UnescapeDataString( uri.Path );
            return Path.GetFullPath( path );
        }

        internal static string[] GetBinPaths()
        {
            /**
             * Finding the bin/Debug path isn't easy:
             * - Paths obtained in the various Executing/calling/entry assemblies
             *   are of little use (Executing/calling assembly is isolated in the ShadowCache
             *   of the VS Designer in AppData\Local\Microsoft\VisualStudio\12.0\Designer\ShadowCache)
             * - Same deal for AppDomain paths.
             * - Entry assembly doesn't help either (It's Visual Studio itself, for heavens's sake).
             * - Environment.CurrentDirectory is completely unreliable:
             *   Visual Studio does not change it and will keep its current one when it launches, 
             *   effectively setting it in System32 when lauching without clicking on the SLN directly.
             * 
             * What we have left is to get all directories containing the AppDomain assemblies.
             * */

            string[] binPaths =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Select( x => GetAssemblyDirectory( x ) )
                    .Distinct()
                    .ToArray();

            return binPaths;
        }

        internal static string[] GetYodiiAssemblyPaths()
        {
            string[] binPaths =
                AppDomain.CurrentDomain.GetAssemblies()
                    .Where( x => x.FullName.StartsWith( "Yodii" ) )
                    .Select( x => GetAssemblyPath( x ) )
                    .Distinct()
                    .ToArray();

            return binPaths;
        }

    }
}
#endif