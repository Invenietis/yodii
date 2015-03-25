#if DEBUG
using System;
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
            // We have to rely on the CurrentDirectory, (probably) set at the solution root, and a hardcoded path. :( 
            string binPath = Path.Combine( Environment.CurrentDirectory, "ObjectExplorer/Yodii.ObjectExplorer/bin/Debug" );

            if( Directory.Exists( binPath ) )
            {
                Discoverer = new StandardDiscoverer( binPath );

                Discoverer.ReadAssembly( Assembly.GetExecutingAssembly().Location );

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
            else
            {
                MessageBox.Show( "Can't start design-time engine. Can't find assembly path " + binPath );
            }
        }

        internal static string AssemblyDirectory
        {
            get
            {
                return GetAssemblyDirectory( Assembly.GetExecutingAssembly() );
            }
        }

        internal static string GetAssemblyDirectory( Assembly a )
        {
            string codeBase = a.CodeBase;
            UriBuilder uri = new UriBuilder( codeBase );
            string path = Uri.UnescapeDataString( uri.Path );
            return Path.GetDirectoryName( path );
        }

    }
}
#endif