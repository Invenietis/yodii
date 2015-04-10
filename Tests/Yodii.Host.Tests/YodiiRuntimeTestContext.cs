using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    /// <summary>
    /// Wraps and starts a Yodii engine, which is then stopped on Dispose.
    /// </summary>
    public class YodiiRuntimeTestContext : IDisposable
    {
        bool disposed = false;

        public IYodiiEngineExternal Engine { get; private set; }
        public IYodiiEngineHost Host { get; private set; }

        public YodiiRuntimeTestContext( IDiscoveredInfo discoveredInfo, YodiiConfiguration configuration )
        {
            if( discoveredInfo == null ) { throw new ArgumentNullException( "discoveredInfo" ); }

            Host = new YodiiHost();
            Engine = new YodiiEngine( Host );

            IYodiiEngineResult result = Engine.Configuration.SetDiscoveredInfo( discoveredInfo );
            Assert.That( result.Success, Is.True );

            if( configuration != null )
            {
                result = Engine.Configuration.SetConfiguration( configuration );
                Assert.That( result.Success, Is.True );
            }

            result = Engine.StartEngine();
            Assert.That( result.Success, Is.True );
        }

        public YodiiRuntimeTestContext( IDiscoveredInfo discoveredInfo )
            : this( discoveredInfo, null )
        {

        }

        public YodiiRuntimeTestContext( YodiiConfiguration config )
            : this( TestHelper.GetDiscoveredInfoInCallingAssembly(), config )
        {

        }

        public YodiiRuntimeTestContext()
            : this( TestHelper.GetDiscoveredInfoInCallingAssembly(), null )
        {

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

        public static T InteractWithPluginDirectly<T>( this YodiiRuntimeTestContext @this ) where T : class, IYodiiPlugin
        {
            object pluginObject = @this.InteractWithPluginDirectly( typeof( T ).FullName );

            Assert.That( pluginObject, Is.InstanceOf<T>(), "RealPluginObject must be castable as IYodiiPlugin class." );

            return (T)pluginObject;
        }

        static IYodiiPlugin InteractWithPluginDirectly( this YodiiRuntimeTestContext @this, string pluginFullName )
        {
            ILivePluginInfo pluginInfo = @this.Engine.LiveInfo.FindPlugin( pluginFullName );

            if( pluginInfo == null ) return null;

            /*
             * Warning!
             * 
             * Interacting directly with Yodii plugins is potentially dangerous.
             * Yodii provides failsafe proxies that you don't get with direct interactions,
             * which can at worst break your application.
             * 
             * If you wish to directly interact with the outside world, use your own plugin
             * and communication methods.
             * 
             * This shouldn't be done outside tests or debugging.
             */

            Assert.That( @this.Host, Is.InstanceOf<YodiiHost>(), "Cannot interact with plugins directly without using the supported Yodii host." );

            YodiiHost h = (YodiiHost)@this.Host;

            IPluginProxy px = h.FindLoadedPlugin( pluginInfo.FullName );

            Assert.That( px, Is.Not.Null, "Host mustn't give a null proxy is plugin is already loaded." );

            return px.RealPluginObject;
        }




        public static YodiiRuntimeTestContext StartService( this YodiiRuntimeTestContext @this, string serviceFullName )
        {
            var result = @this.Engine.StartService( serviceFullName );

            Assert.That( result.Success, Is.True, String.Format( "Service {0} failed to start.", serviceFullName ) );

            return @this;
        }

        public static YodiiRuntimeTestContext StartService<T>( this YodiiRuntimeTestContext @this ) where T : IYodiiService
        {
            return @this.StartService( typeof( T ).FullName );
        }


        public static ILiveServiceInfo FindLiveService( this YodiiRuntimeTestContext @this, string serviceFullName )
        {
            return @this.Engine.LiveInfo.FindService( serviceFullName );
        }

        public static ILiveServiceInfo FindLiveService<T>( this YodiiRuntimeTestContext @this ) where T : IYodiiService
        {
            return @this.Engine.LiveInfo.FindService( typeof( T ).FullName );
        }

        public static T InteractWithServiceDirectly<T>( this YodiiRuntimeTestContext @this ) where T : IYodiiService
        {
            ILiveServiceInfo serviceInfo = @this.Engine.LiveInfo.FindService( typeof( T ).FullName );

            if( serviceInfo == null ) return default( T );

            object pluginObject = @this.InteractWithPluginDirectly( serviceInfo.RunningPlugin.FullName );

            Assert.That( pluginObject, Is.InstanceOf<T>(), "RealPluginObject must be castable as IYodiiService class." );

            return (T)pluginObject;
        }
    }
}
