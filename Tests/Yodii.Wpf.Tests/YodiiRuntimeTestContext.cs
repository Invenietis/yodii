using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.Wpf.Tests
{
    /// <summary>
    /// Wraps and starts a Yodii engine, which is then stopped on Dispose.
    /// </summary>
    public class YodiiRuntimeTestContext : IDisposable
    {
        bool disposed = false;

        public IYodiiEngineExternal Engine { get; private set; }
        public IYodiiEngineHost Host { get; private set; }

        public YodiiRuntimeTestContext( IDiscoveredInfo discoveredInfo )
        {
            if( discoveredInfo == null ) { throw new ArgumentNullException( "discoveredInfo" ); }

            Host = new YodiiHost();
            Engine = new YodiiEngine( Host );

            Engine.Configuration.SetDiscoveredInfo( discoveredInfo );

            IYodiiEngineResult result = Engine.StartEngine();
            Assert.That( result.Success );
        }

        public YodiiRuntimeTestContext( Assembly yodiiAssembly )
            : this( TestHelper.GetDiscoveredInfoInAssembly( yodiiAssembly ) )
        {

        }

        public YodiiRuntimeTestContext()
            : this( TestHelper.GetDiscoveredInfoInCallingAssembly() )
        {

        }

        public static YodiiRuntimeTestContext FromEmptyDiscoveredInfo()
        {
            return new YodiiRuntimeTestContext( TestHelper.GetEmptyDiscoveredInfo() );
        }

        public void Dispose()
        {
            if( !disposed )
            {
                Engine.StopEngine();

                disposed = true;
            }
        }


    }

    public static class YodiiRuntimeTestContextExtensions
    {
        public static YodiiRuntimeTestContext StartPlugin( this YodiiRuntimeTestContext @this, string pluginFullName )
        {
            var result = @this.Engine.StartPlugin( pluginFullName );

            Assert.That( result.Success, Is.True, String.Format( "Plugin {0} failed to start.", pluginFullName ) );

            return @this;
        }
        public static YodiiRuntimeTestContext StartPlugin<T>( this YodiiRuntimeTestContext @this ) where T : IYodiiPlugin
        {
            return @this.StartPlugin( typeof( T ).FullName );
        }

        public static ILivePluginInfo FindLivePlugin( this YodiiRuntimeTestContext @this, string pluginFullName )
        {
            return @this.Engine.LiveInfo.FindPlugin( pluginFullName );
        }
        public static ILivePluginInfo FindLivePlugin<T>( this YodiiRuntimeTestContext @this ) where T : IYodiiPlugin
        {
            return @this.Engine.LiveInfo.FindPlugin( typeof( T ).FullName );
        }
    }
}
