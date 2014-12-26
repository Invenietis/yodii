using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Engine.Tests.Mocks;
using Yodii.Model;

namespace Yodii.Engine.Tests.ConfigurationSolverTests
{
    [TestFixture]
    class CurrentFailureTests
    {
        [Test]
        public void DynamicInvalidLoop()
        {
            #region graph
            /*
            *                  +--------+  
            *      +-----------|Service1|  
            *      |           |Running |  
            *      |           +---+----+  
            *  +---+---------+        | 
            *  |Service1.1   |        |    
            *  |Optional     |  +---+-----+
            *  +------------++  |Plugin1.2|
            *      |        |   |Optional |-----------------------+ 
            *      |        |   +---------+                       |
            *   +--+------+ |                                     |
            *   |Plugin1.1| |                                     |Running
            *   |Optional | |                                     |
            *   +---------+ |                      +---------+    | 
            *               |            +---------+Service2 |<---+ 
            *               |         +--+------+  |Optional |      
            *               +---------+Plugin2.1|  +----+----+      
            *                         |Optional |       |           
            *                         +---------+       |           
            *                                        +--+------+    
            *                                        |Plugin2.2|    
            *                                        |Optional |    
            *                                        +---------+
            */
            #endregion

            StaticConfigurationTests.CreateDynamicInvalidLoop().FullStart( ( engine, res ) =>
            {
                engine.StartPlugin( "Plugin1.1", StartDependencyImpact.Minimal ).CheckSuccess( "Starting the Plugin1.1 (just for fun)." );
                engine.StartPlugin( "Plugin1.2", StartDependencyImpact.Minimal ).CheckSuccess( "Starting Plugin1.2 (this launch the Plugin2.2 to support Service2." );
                Assert.That( engine.LiveInfo.FindPlugin( "Plugin2.2" ).RunningStatus, Is.EqualTo( RunningStatus.Running ), "Plugin2.2 is required by Plugin1.2." );

                engine.StopPlugin( "Plugin2.2" ).CheckSuccess( "Stops the Plugin2.2 will stop the Service2 => all will be stopped." );

                engine.CheckAllPluginsStopped( "Plugin2.2, Plugin1.2" );
            } );
        }
        #region Invalid Loops with runnable/optionnal references which we don't want to solve for the moment.
        [Test]
        public void InvalidInternalLoop1WithARunnableReference()
        {
            #region graph
            /**
             *                    +----------+           
             *        +---------->| Service1 +-------+   
             *        |           | Optional |       |   
             *        |           +----------+       |   
             *        |                          +---+------+                  
             *        |                          | Plugin1  |                  
             *        |                          | Optional |                  
             *  +-----+------+                   +---+------+                  
             *  | Service1.1 |                       | Runnable                       
             *  |  Optional  |-----------------------+                        
             *  +----+-------+ 
             *       |         
             *       |                 
             *       |         
             *       |         
             *  +----+-------+                
             *  | Plugin1.1  |                
             *  | Optional   |                
             *  +------------+  
             *                                                            
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.1", d.DefaultAssembly ) );
            d.FindService( "Service1.1" ).Generalization = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin1.1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1.1" ).Service = d.FindService( "Service1.1" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service1.1" ), DependencyRequirement.Runnable );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( d );



            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled( "Plugin1" );
                res.CheckAllPluginsRunnable( "Plugin1.1" );
                res.CheckAllServicesRunnable( "Service1,Service1.1" );
            } );
        }


        [Test]
        public void InvalidLoop2WithARunnableReference()
        {
            #region graph
            /**
             *                  +--------+                              +--------+
             *      +---------->|Service1+-------+   *----------------->|Service2|---+
             *      |           |Optional|       |   | Need Runnable    |Optional|   |
             *      |           +---+----+       |   |                  +---+----+   |
             *      |               |        +---+-----+                    |        |
             *      |               |        |Plugin1  |                    |        |
             *      |               |        |Optional |                    |        |
             *  +---+------+        |        +---+-----+                    |        |
             *  |Service1.1|        |                                       |        |
             *  |Optional  |-----------------+                              |        |
             *  +----+-----+        |        |                          +---+-----+  |
             *       |              |        ---------------------------|Plugin2  |  |
             *       |              |                   Need Running    |Optional |  |
             *       |          +---+------+                            +---------+  |
             *       |          |Plugin1bis|Need Running                             |
             *       |          |Optional  |--------+                                |
             *       |          +----------+        |                   +--------+   |
             *  +---+-------+                       --------------------|Service3|   |
             *  |Plugin1.1  |                                           |Optional|   |
             *  |Optional   |                                           +---+----+   |
             *  +-----------+                                               |        |
             *                                                              |        |
             *                                                          +---+-----+  |
             *                                                          |Plugin3  |--+
             *                                                          |Optional | Need Running 
             *                                                          +---------+  
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );
            d.FindService( "Service1.1" ).Generalization = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin1bis", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1bis" ).Service = d.FindService( "Service1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin1.1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1.1" ).Service = d.FindService( "Service1.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service2" );
            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service3" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2" ), DependencyRequirement.Runnable );
            d.FindPlugin( "Plugin1bis" ).AddServiceReference( d.FindService( "Service3" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service1.1" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service2" ), DependencyRequirement.Running );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( d );



            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled( "Plugin1,Plugin1bis" );
                res.CheckAllPluginsRunnable( "Plugin2,Plugin1.1,Plugin3" );
                System.Diagnostics.Debug.WriteLine( res.StaticSolvedConfiguration.FindPlugin( "Plugin2" ).FinalConfigSolvedStatus.ToString() );
                res.CheckAllServicesRunnable( "Service1,Service1.1,Service2,Service3" );
            } );
        }
        [Test]
        public void InvalidLoop3WithARunnableReference()
        {
            #region graph
            /**
             *                         +--------+             
             *                         |Service2+-------------+
             *                         |Optional|             |
             *                         +---+----+             |
             *                             |                  |                +----------+
             *                             |                  +--------------->|Service2.2|
             *                             |                                   |Optional  |
             *  +---+------+               |                                   +-+------+-+
             *  |Service1  |               |                                     |      |       
             *  |Optional  |               |                     +---------------+      |       
             *  +----+-----+               |                     |                      |       
             *       |                     |                     |                      |       
             *       |                     |                     |                      |       
             *       |                 +---+------+              |                      |       
             *       |    Need Running |Service2.1|              |                  +---+-----+                           
             *       |    +----------->|Optional  |              |                  |Plugin2.2|             
             *       |    |            +----------+              |                  |Optional |             
             *  +----+----+-+               |                    |                  +---------+             
             *  |Plugin1    |               |                    |            
             *  |Optional   |               |                    |            
             *  +---------+-+           +---+------+             |            
             *            |             |Plugin2.1 |             |            
             *            |             |Optional  |             |            
             *            |             +----------+             |            
             *            |                                      |            
             *            |                                      |            
             *            |                                      |
             *            |Need Runnable+---+------+             |
             *            +------------>|Service3  |             |
             *                          |Optional  |             |
             *                          +----------+             |
             *                               |                   |
             *                               |                   |
             *                               |                   |
             *                           +---+------+            |
             *                           |Plugin3   |------------+   
             *                           |Optional  |  Need Running 
             *                           +----------+   
             * 
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );
            d.FindService( "Service2.1" ).Generalization = d.FindService( "Service2" );
            d.FindService( "Service2.2" ).Generalization = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2.1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2.1" ).Service = d.FindService( "Service2.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2.2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2.2" ).Service = d.FindService( "Service2.2" );
            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service3" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2.1" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service3" ), DependencyRequirement.Runnable );
            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service2.2" ), DependencyRequirement.Running );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( d );

            var result = engine.StartEngine();
            engine.FullStaticResolutionOnly( res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled( "Plugin1" );
                res.CheckAllPluginsRunnable( "Plugin2.1,Plugin2.2,Plugin3" );
                res.CheckAllServicesRunnable( "Service2,Service2.1,Service2.2,Service3" );
                res.CheckAllServicesDisabled( "Service1" );
            } );
        }
        #endregion
    }
}
