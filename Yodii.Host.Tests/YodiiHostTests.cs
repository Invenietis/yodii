using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;
using System.Reflection;
using NUnit.Framework;
using Yodii.Model;
using Yodii.Engine;
using Yodii.Discoverer;
using CK.Core;
using System.IO;

namespace Yodii.Host.Tests
{
    [TestFixture]
    public class YodiiHostTests
    {
        [Test]
        public void ToSeeWhatHappensChoucrouteTest1()
        {

            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Host.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost(); /*IYodiiEngineHost this is not enough, need access to PluginCreator & ServiceReferencesBinder*/
            YodiiEngine engine = new YodiiEngine( host);
            engine.SetDiscoveredInfo(info);

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.Host.Tests.ChoucroutePlugin", ConfigurationStatus.Running );
            
            host.PluginCreator = Runner.CreatePlugin;


            #region Action: ConfigureServiceReferences
            Action<ICKReadOnlyCollection<IPluginProxy>> ConfigureServiceReferences = delegate( ICKReadOnlyCollection<IPluginProxy> newPluginsLoaded )
               {
                   HashSet<PropertyInfo> processedProperties = new HashSet<PropertyInfo>();
                   foreach( var p in newPluginsLoaded )
                   {
                       processedProperties.Clear();
                       Type pType = p.RealPluginObject.GetType();

                       foreach( IServiceReferenceInfo r in p.PluginKey.ServiceReferences )
                       {
                           PropertyInfo pService = pType.GetProperty( r.ConstructorParameterName );
                           processedProperties.Add( pService );
                           //Debug.Assert( !r.IsIServiceWrapped || r.Reference.IsDynamicService, "IService<T> => T is IDynamicService." );
                           //if( r.Reference.IsDynamicService )
                           //{
                               object refService;
                               Type serviceType = pService.PropertyType;
                               if( !r.IsNakedRunningService/*IsIServiceWrapped*/ )
                               {
                                   // Extracts the actual service type.
                                   Debug.Assert( serviceType.IsGenericType && serviceType.GetGenericTypeDefinition() == typeof( IService<> ) );
                                   serviceType = serviceType.GetGenericArguments()[0];
                                   // Ensures that the proxy exists.
                                   refService = host.ServiceHost.EnsureProxyForDynamicService( serviceType );
                               }
                               else
                               {
                                   // Not IService<> wrapped: sets it only if it is running (null otherwise).
                                   refService = host.ServiceHost.GetRunningProxy( serviceType );
                               }
                               pService.SetValue( p.RealPluginObject, refService, null );
                           //}
                           //else
                           //{
                           //    InjectExternalService( pService, p.RealPluginObject );
                           //}
                           
                           //I just did nonsense...
                       }
                   }
               };
            #endregion


            host.ServiceReferencesBinder=ConfigureServiceReferences;
             
            var result = engine.Start();

            engine.FullStart( ( YodiiEngine engine2, IYodiiEngineResult res ) =>
            {               
                engine.LiveInfo.FindPlugin( discoverer.GetDiscoveredInfo().PluginInfos[0].PluginFullName ).Start();
                engine.CheckAllPluginsRunning( discoverer.GetDiscoveredInfo().PluginInfos[0].PluginFullName );
                engine.LiveInfo.FindPlugin( discoverer.GetDiscoveredInfo().PluginInfos[0].PluginFullName ).Stop();
                engine.CheckAllPluginsStopped( discoverer.GetDiscoveredInfo().PluginInfos[0].PluginFullName );
            });
        }
    }

    public class Runner
    {
        public static IYodiiPlugin CreatePlugin( IPluginInfo info )
        {
            return WrapResolveAssembly( () =>
            {
                //Assembly a = Assembly.Load( info.AssemblyInfo.AssemblyName );
                Debug.Assert( info.AssemblyInfo.AssemblyLocation != null, "the assemblyLocation is null !" );
                //Assembly a = Assembly.LoadFile( /*info.AssemblyInfo.AssemblyLocation.AbsolutePath*/ Path.GetFullPath("Yodii.Host.Tests.dll") );
                Assembly a = Assembly.LoadFile(  Path.GetFullPath( info.AssemblyInfo.AssemblyName+".dll" ));//.dll
                Type t = a.GetType( info.PluginFullName, true );
                var cSP = t.GetConstructor( new Type[] { typeof( IServiceProvider ) } );
                //if( cSP != null ) return (IYodiiPlugin)cSP.Invoke( new object[] { _contextObject } );
                return Activator.CreateInstance( t ) as IYodiiPlugin;
            } );
        }
        static IYodiiPlugin WrapResolveAssembly( Func<IYodiiPlugin> creator )
        {
            IYodiiPlugin instance = null;
            AppDomain domain = AppDomain.CurrentDomain;
            domain.AssemblyResolve += OnAssemblyResolve;

            instance = creator();

            domain.AssemblyResolve -= OnAssemblyResolve;
            return instance;
        }
        static Assembly OnAssemblyResolve( object sender, ResolveEventArgs args )
        {
            AssemblyName name = new AssemblyName( args.Name );
            //if( name.Name == "CK.Plugin.Config.Model" ) return typeof( CK.Plugin.Config.IPluginConfigAccessor ).Assembly;
            //if( name.Name == "CK.Plugin.Model" ) return typeof( CK.Plugin.IPlugin ).Assembly;
            return null;
        }
    }
}
