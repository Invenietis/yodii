#if DEBUG
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using CK.Core;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Mocks
{
    /// <summary>
    /// Helper utility used to start and handle a Yodii Engine at design time.
    /// </summary>
    [ExcludeFromCodeCoverage]
    static class MockEngineHelper
    {
        internal static YodiiEngine Engine { get; private set; }
        internal static StandardDiscoverer Discoverer { get; private set; }
        internal static YodiiHost Host { get; private set; }

        static IActivityMonitor _m;

        internal static void StartDesignTimeYodiiEngine()
        {
            _m = new ActivityMonitor();

            Host = new YodiiHost( _m );
            Engine = new YodiiEngine( Host );

            // At design time, assemblies are copied somewhere in AppData\Local\Microsoft\VisualStudio\12.0\Designer\ShadowCache.
            // This makes the resolution somewhat difficult, as each assembly is copied in a subfolder without its dependencies.
            string[] binPaths = GetBinPaths();

            Discoverer = new StandardDiscoverer( binPaths );

            string[] paths = GetYodiiAssemblyPaths();

            //MessageBox.Show( String.Format( "Loading assemblies:\n\n{0}", String.Join( "\n", paths ) ) );

            foreach( string ap in paths )
            {
                _m.Trace().Send( "Opening {0}", ap );
                Discoverer.ReadAssembly( ap );
            }

            IDiscoveredInfo info = Discoverer.GetDiscoveredInfo();
            Engine.Configuration.SetDiscoveredInfo( info );

            IYodiiEngineResult result = Engine.StartEngine();
            if( result.Success == false )
            {
                _m.Error().Send( "Failed to start design-time engine." );
                MessageBox.Show( "Couldn't start engine." );
            }
            else
            {
                //MessageBox.Show( "Started design-time engine.\nPlugins:" + String.Join("\n", Engine.LiveInfo.Plugins.Select( p => p.FullName )) );
            }
        }

        internal static string GetAssemblyDirectory( Assembly a )
        {
            try
            {
                string codeBase = a.CodeBase;
                UriBuilder uri = new UriBuilder( codeBase );
                string path = Uri.UnescapeDataString( uri.Path );
                return Path.GetDirectoryName( path );
            }
            catch( Exception e )
            {
                _m.Error().Send( e, "Error while getting Assembly Directory of {0}.", a.FullName );
            }

            return String.Empty;
        }

        internal static string GetAssemblyPath( Assembly a )
        {
            try
            {
                string codeBase = a.CodeBase;
                UriBuilder uri = new UriBuilder( codeBase );
                string path = Uri.UnescapeDataString( uri.Path );
                return Path.GetFullPath( path );
            }
            catch( Exception e )
            {
                _m.Error().Send( e, "Error while getting Assembly Path of {0}.", a.FullName );
            }

            return null;
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
                BuildAssemblyMapFromAppDomain().Values
                    .Where( x => x.FullName.StartsWith( "Yodii.ObjectExplorer" ) )
                    .Select( x => GetAssemblyPath( x ) )
                    .Distinct()
                    .ToArray();

            return binPaths;
        }

        /// <summary>
        /// Builds an assembly map containing only the most recent assemblies (by file creation date) for each FullName from application domain.
        /// </summary>
        /// <returns>Dictionary mapping FullName to Assembly.</returns>
        internal static IDictionary<string, Assembly> BuildAssemblyMapFromAppDomain()
        {
            Dictionary<string, Assembly> map = new Dictionary<string, Assembly>();
            Dictionary<string, FileInfo> fileMap = new Dictionary<string, FileInfo>();

            foreach( Assembly a in AppDomain.CurrentDomain.GetAssemblies() )
            {
                Assembly existingAssembly;
                if( map.TryGetValue( a.FullName, out existingAssembly ) )
                {
                    FileInfo existingFile = fileMap[a.FullName];

                    string path = GetAssemblyPath( a );
                    if( path != null )
                    {
                        FileInfo newAssemblyFile = new FileInfo( path );

                        if( newAssemblyFile.LastWriteTimeUtc > existingFile.LastWriteTimeUtc )
                        {
                            map[a.FullName] = a;
                            fileMap[a.FullName] = newAssemblyFile;
                        }
                    }
                }
                else
                {
                    string path = GetAssemblyPath( a );
                    if( path != null )
                    {
                        FileInfo newAssemblyFile = new FileInfo( path );
                        map[a.FullName] = a;
                        fileMap[a.FullName] = newAssemblyFile;
                    }
                }
            }

            return map;
        }

    }
}
#endif