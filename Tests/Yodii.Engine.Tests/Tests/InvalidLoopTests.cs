using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using NUnit.Framework;
using Yodii.Engine.Tests.Mocks;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    public class InvalidLoopTests
    {
        [Test]
        public void running_plugin_blocks_if_dependency_must_start_or_is_runnable()
        {
            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Running, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" )
                .CheckItem( "Plugin1", p => p == null, "When static resoltuion fails, LiveInfo is not available." );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.RunnableRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Runnable, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.IsStartOptionalRecommended )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );
        }
        
        [Test]
        public void running_plugin_runs_and_disables_the_reference()
        {

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );
            

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.TryStartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );

            BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.TryFullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );
        }

        [Test]
        public void runnable_plugin_blocks_if_dependency_must_start()
        {
            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.Running, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" )
                .CheckItem( "Plugin1", p => p == null, "When static resoltuion fails, LiveInfo is not available." );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.Optional, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );
        }

        [Test]
        public void runnable_plugin_can_run_but_starting_the_dependency_stops_it()
        {
            //
            // Suicide is allowed for the following cases: starting Plugin2 will stop Plugin1.
            //
            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.RunnableRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.Runnable, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.Optional, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.OptionalRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.Optional, StartDependencyImpact.TryStartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Runnable, DependencyRequirement.OptionalRecommended, StartDependencyImpact.TryFullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );
        }

        [Test]
        public void optional_plugin_does_not_block_but_is_disabled_if_dependency_must_start()
        {
            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.Running, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.RunnableRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.RunnableRecommended, StartDependencyImpact.IsStartRunnableRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.IsStartOptionalRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.Runnable, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.Optional, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );
        }

        [Test]
        public void optional_plugin_can_run_but_starting_the_dependency_stops_it()
        {
            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.Optional, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.Optional, StartDependencyImpact.TryStartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.TryFullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );
        }

        /**
        *                    +----------+           
        *        +---------->| Service1 +-------+   
        *        |           | Optional |       |   
        *        |           +----------+       |   
        *        |                          +---+-----------+                  
        *        |                          | Plugin1       |                  
        *        |                          |               |                  
        *        |                          | plugin1Config |                  
        *        |                          |       &       |                  
        *        |                          | plugin1Impact |                  
        *  +-----+------+                   +---+-----------+                  
        *  |  Service2  |                       | fromPlugin1ToService2                       
        *  |  Optional  |-----------------------+                        
        *  +----+-------+ 
        *       |         
        *       |                 
        *       |         
        *       |         
        *  +----+-------+                
        *  | Plugin2    |                
        *  | Optional   |                
        *  +------------+  
        *                                                            
        */
        YodiiEngine BuildSimpleLoop( ConfigurationStatus plugin1Config, DependencyRequirement fromPlugin1ToService2, StartDependencyImpact plugin1Impact )
        {
            var d = new DiscoveredInfo();
            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.FindService( "Service2" ).Generalization = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service2" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2" ), fromPlugin1ToService2 );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( d );
            engine.Configuration.Layers.Default.Items.Set( "Plugin1", plugin1Config, plugin1Impact );

            return engine;
        }

        [Test]
        public void starts_and_stops_with_another_loop()
        {
            #region graph
            /*
            *                  +-----------------+  
            *      +-----------|     Service1    |  
            *      |           |  service1Config |  
            *      |           +------+----------+  
            *  +---+---------+        | 
            *  |Service1.1   |        |    
            *  |Optional     |  +---+-----+
            *  +------------++  |Plugin1.2|
            *      |        ^   |Optional |-----------------------+ 
            *      |        |   +---------+                       |
            *   +--+------+ |                                     |
            *   |Plugin1.1| |                                     |Running
            *   |Optional | |                                     |
            *   +---------+ |                      +---------+    |
            *               |            +---------+Service2 |<---+
            *               |            |         |Optional |
            *               |         +--+------+  +----+----+   
            *               +---------+Plugin2.1|       |
            *                Running  |Optional |       |        
            *                         +---------+       |        
            *                                        +--+------+ 
            *                                        |Plugin2.2| 
            *                                        |Optional | 
            *                                        +---------+
            */
            #endregion
            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( MockInfoFactory.CreateGraphDynamicInvalidLoop() );
            RunAnotherLoop( engine, ConfigurationStatus.Optional );
            RunAnotherLoop( engine, ConfigurationStatus.Runnable );
            RunAnotherLoop( engine, ConfigurationStatus.Running );
        }

        static void RunAnotherLoop( YodiiEngine engine, ConfigurationStatus service1Config )
        {
            Assert.That( !engine.IsRunning );
            engine.Configuration.Layers.Default.Items.Set( "Service1", service1Config );
            engine.FullStartAndStop( ( e, res ) =>
            {
                if( service1Config == ConfigurationStatus.Running )
                {
                    e.CheckAllServicesRunningLocked( "Service1" )
                     .CheckAllServicesRunning( "Service2" )
                     .CheckAllServicesStopped( "Service1.1" )
                     .CheckAllPluginsRunning( "Plugin1.2, Plugin2.2" );

                    e.StartPlugin( "Plugin1.1", StartDependencyImpact.Minimal )
                        .CheckSuccess( "Starting Plugin1.1." )
                        .Engine.CheckAllServicesRunningLocked( "Service1" )
                               .CheckAllServicesRunning( "Service1.1" )
                               .CheckAllServicesStopped( "Service2" )
                               .CheckAllPluginsRunning( "Plugin1.1" )
                               .CheckAllPluginsStopped( "Plugin1.2, Plugin2.1, Plugin2.2" );

                    e.StartPlugin( "Plugin2.1", StartDependencyImpact.Minimal )
                        .CheckSuccess( "Starting Plugin2.1." )
                        .Engine.CheckAllServicesRunningLocked( "Service1" )
                               .CheckAllServicesRunning( "Service1.1, Service2" )
                               .CheckAllPluginsRunning( "Plugin1.1, Plugin2.1" )
                               .CheckAllPluginsStopped( "Plugin1.2, Plugin2.2" );
                }
                else
                {
                    e.StartPlugin( "Plugin1.2", StartDependencyImpact.Minimal )
                        .CheckSuccess( "Starting the Plugin1.2: starts Service1, Service 2 and Plugin2.2." )
                        .Engine.CheckAllServicesRunning( "Service1, Service2" )
                               .CheckAllServicesStopped( "Service1.1" )
                               .CheckAllPluginsRunning( "Plugin1.2, Plugin2.2" );

                    e.StartPlugin( "Plugin1.1", StartDependencyImpact.Minimal )
                        .CheckSuccess( "Starting Plugin1.1." )
                        .Engine.CheckAllServicesRunning( "Service1.1, Service1" )
                               .CheckAllServicesStopped( "Service2" )
                               .CheckAllPluginsRunning( "Plugin1.1" )
                               .CheckAllPluginsStopped( "Plugin1.2, Plugin2.1, Plugin2.2" );

                    e.StartPlugin( "Plugin2.1", StartDependencyImpact.Minimal )
                        .CheckSuccess( "Starting Plugin2.1." )
                        .Engine.CheckAllServicesRunning( "Service1, Service1.1, Service2" )
                               .CheckAllPluginsRunning( "Plugin1.1, Plugin2.1" )
                               .CheckAllPluginsStopped( "Plugin1.2, Plugin2.2" );
                }

            } );
            Assert.That( !engine.IsRunning );
        }

        [Test]
        public void more_complex_loop_with_a_runnable()
        {
            BuildMoreComplexLoop( ConfigurationStatus.Optional, StartDependencyImpact.Unknown )
                .FullStartAndStop( ( e, res ) =>
                {
                    res.CheckSuccess();
                    e.CheckAllPluginsDisabled( "Plugin1bis" )
                     .CheckAllPluginsStopped( "Plugin1, Plugin1.1, Plugin2, Plugin3" )
                     .CheckAllServicesStopped( "Service1, Service1.1, Service2, Service3" );
                    Assert.That( e.LiveInfo.FindPlugin( "Plugin1" ).Capability.CanStart );
                    Assert.That( e.LiveInfo.FindPlugin( "Plugin1" ).Capability.CanStartWithStartRecommended );
                    Assert.That( e.LiveInfo.FindPlugin( "Plugin1" ).Capability.CanStartWith( StartDependencyImpact.IsStartRunnableOnly ), Is.False );
                    Assert.That( e.LiveInfo.FindPlugin( "Plugin1" ).Capability.CanStartWithFullStart == false );
                    Assert.Throws<InvalidOperationException>( () => e.StartPlugin( "Plugin1", StartDependencyImpact.FullStart ) );

                    Assert.That( e.LiveInfo.FindService( "Service1" ).Capability.CanStart );
                    Assert.That( e.LiveInfo.FindService( "Service1" ).Capability.CanStartWithFullStart, "Since Plugin1.1 is available and has no dependency." );
                    Assert.That( e.LiveInfo.FindService( "Service1" ).Capability.CanStartWithStartRecommended );

                    e.StartPlugin( "Plugin1" )
                        .CheckSuccess()
                        .Engine
                            .CheckAllPluginsRunning( "Plugin1" )
                            .CheckAllServicesRunning( "Service1" );

                    Assert.That( e.LiveInfo.FindService( "Service2" ).Capability.CanStart, "Nothing prevents Service2 to start..." );

                    // Plugin1 is stopped whenever its runnable dependeny is started.
                    e.StartService( "Service2" )
                        .CheckSuccess()
                        .Engine
                            .CheckAllPluginsStopped( "Plugin1, Plugin3" )
                            .CheckAllPluginsRunning( "Plugin1.1, Plugin2" )
                            .CheckAllServicesRunning( "Service1, Service1.1, Service2" )
                            .CheckAllServicesStopped( "Service3" );
                } );
        }

        [Test]
        public void more_complex_loop_with_a_runnable_required_that_disable_the_plugin()
        {
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.IsStartRunnableOnly );
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartOptionalOnly );
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartOptionalOnly | StartDependencyImpact.IsStartOptionalRecommended );
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartOptionalOnly | StartDependencyImpact.IsStartRunnableRecommended );
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartRunnableRecommended );
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.IsStartRunnableOnly | StartDependencyImpact.IsStartRunnableRecommended | StartDependencyImpact.IsStartOptionalRecommended );
            RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact.FullStart );
        }

        static void RunMoreComplexLoopWithRequiredRunnable( StartDependencyImpact service1Impact )
        {
            BuildMoreComplexLoop( ConfigurationStatus.Running, service1Impact )
                .FullStartAndStop( ( e, res ) =>
                {
                    res.CheckSuccess();
                    
                    e.CheckAllPluginsDisabled( "Plugin1, Plugin1bis" )
                     .CheckAllPluginsRunningLocked( "Plugin1.1" )
                     .CheckAllPluginsStopped( "Plugin2, Plugin3" )
                     .CheckAllServicesRunningLocked( "Service1, Service1.1" )
                     .CheckAllServicesRunning( "" )
                     .CheckAllServicesStopped( "Service2, Service3" );

                    e.StartService( "Service3" )
                        .CheckSuccess()
                        .Engine
                            .CheckAllPluginsDisabled( "Plugin1, Plugin1bis" )
                            .CheckAllPluginsRunningLocked( "Plugin1.1" )
                            .CheckAllPluginsRunning( "Plugin2, Plugin3" )
                            .CheckAllServicesRunningLocked( "Service1, Service1.1" )
                            .CheckAllServicesRunning( "Service2, Service3" );
                } );
        }

        static YodiiEngine BuildMoreComplexLoop( ConfigurationStatus service1Config, StartDependencyImpact service1Impact )
        {
            #region graph
            /**
             *                  +----------+             (suicide here!)   +----------+
             *        +-------->| Service1 +--------+   *----------------->| Service2 |----+
             *        |         | Optional |        |   | Need Runnable    | Optional |    |
             *        |         +-----+----+        |   |                  +---+------+    |
             *        |               |        +----+-----+                    |           |
             *        |               |        | Plugin1  |                    |           |
             *        |               |        | Optional |                    |           |
             *  +-----+------+        |        +----------+                    |           |
             *  | Service1.1 |        |                                        |           |
             *  | Optional   |-----------------+                               |           |
             *  +----+-------+        |        |                         +-----+----+      |
             *       |                |        --------------------------| Plugin2  |      |
             *       |                |                   Need Running   | Optional |      |
             *       |           +----+-------+                          +----------+      |
             *       |           | Plugin1bis | Need Running                               |
             *       |           | Optional   |--------+                                   |
             *       |           +------------+        |                 +----------+      |
             *  +----+------+          ||              +-----------------| Service3 |      |
             *  | Plugin1.1 |          ||                                | Optional |      |
             *  | Optional  |       Disabled                             +-----+----+      |
             *  +-----------+     (InvalidLoop)                                |           |
             *                                                                 |           |
             *                                                            +---+------+     |
             *                                                            | Plugin3  |-----+
             *                                                            | Optional | Need Running 
             *                                                            +----------+  
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
            engine.Configuration.Layers.Default.Items.Set( "Service1", service1Config, service1Impact );
            return engine;
        }


        [Test]
        public void quite_complex_loop_dynamic_start_scenario_1()
        {
            BuildQuiteComplexLoop()
                .FullStartAndStop( ( e, res ) =>
                {
                    res.CheckSuccess();
                    e.CheckAllPluginsDisabled( ">Plugin1bis" );
                    e.CheckAllServicesStopped( "Service1, Service2, Service2.1, Service2.2, Service2.3, Service3, >Service1, >Service1.1, >Service2, >Service3" );
                    e.CheckAllPluginsStopped( "Plugin1, Plugin2.1, Plugin2.2, Plugin3, >Plugin1, >Plugin1.1, >Plugin2, >Plugin3" );

                    e.StartService( "Service1", StartDependencyImpact.StartRecommended ).CheckSuccess();
                    e.CheckAllPluginsDisabled( ">Plugin1bis" );
                    e.CheckAllPluginsRunning( "Plugin1, Plugin2.1, Plugin3" );
                    e.CheckAllPluginsStopped( "Plugin2.2, >Plugin1, >Plugin1.1, >Plugin2, >Plugin3" );
                    e.CheckAllServicesRunning( "Service1, Service2.3, Service2.2, Service2.1, Service2, Service3" );

                } );
        }

        [Test]
        public void quite_complex_loop_static_constraints_1()
        {
            var engine = BuildQuiteComplexLoop();
            engine.Configuration.Layers.Default.Items.Set( "Service1", ConfigurationStatus.Running, StartDependencyImpact.StartRecommended );
            engine.FullStartAndStop( ( e, res ) =>
                {
                    res.CheckSuccess();
                    e.CheckAllPluginsDisabled( ">Plugin1bis, Plugin2.2, >Plugin1, >Plugin1.1, >Plugin2, >Plugin3" );
                    e.CheckAllPluginsRunningLocked( "Plugin1, Plugin2.1, Plugin3" );
                    e.CheckAllServicesRunningLocked( "Service1, Service2.3, Service2.2, Service2.1, Service2, Service3" );
                    e.CheckAllServicesDisabled( ">Service1, >Service1.1, >Service2, >Service3" );

                    e.CheckAllPluginsStopped( "" ).CheckAllPluginsRunning( "" );
                    e.CheckAllServicesStopped( "" ).CheckAllServicesRunning( "" );
                } );
        }

        static YodiiEngine BuildQuiteComplexLoop()
        {
            #region graph
            /**
             *                                                             +----------------+             
             *                                                             |    Service2    |
             *                                                             |    Optional    |
             *                                                             +-+-------+---+--+
             *                                                               |       |   |
             *                                                               |       |   |
             *                                                   +-----------+--+    |   |
             *                                                   |  Service2.1  |    |   +-----------------------+
             *                                                   |   Optional   |    |                           |
             *                                                   +-+------+-----+    |                           |
             *                                                     |      |          |                           |
             *  +----------+           +------------+              |      |          |                      +----+------+             (suicide here!)   +-----------+                         
             *  | Service1 |           | Service2.2 +--------------+      |          |            +-------->| >Service1 +--------+   *----------------->| >Service2 |----+                                 
             *  | Optional |           |  Optional  |                     |          |            |         |  Optional |        |   | Need Runnable    | Optional  |    |                               
             *  +----+-----+           +-----+------+                 +---+-----+    |            |         +------+----+        |   |                  +----+------+    |                            
             *       |                       |                        |Plugin2.2|    |            |                |        +----+-----+                    |            |                            
             *       |                       |                        |Optional |    |            |                |        | >Plugin1 |                    |            |                             
             *       |                 +-----+------+                 +---------+    |            |                |        | Optional |                    |            |        
             *       |   OptionalReco  | Service2.3 |                                |            |                |        +----------+                    |            |                    
             *       |    +----------->| Optional   |                                |      +-----+-------+        |                                        |            |
             *       |    |            +----+-------+                                |      | >Service1.1 |        |                                        |            |      
             *  +----+----+-+               |                                        |      |  Optional   |-----------------+                               |            |      
             *  | Plugin1   |               |                                        |      +-----+-------+        |        |                         +-----+-----+      |   
             *  | Optional  |               |                                        |           |                 |        --------------------------| >Plugin2  |      |
             *  +---------+-+          +----+------+                                 |           |                 |                   Need Running   | Optional  |      |
             *            |            | Plugin2.1 |                                 |           |                 |                                  +-----------+      |
             *            |            | Optional  |                                 |           |           +-----+-------+                                             |
             *            |            +-----------+                                 |           |           | >Plugin1bis | Need Running                                |
             *            |                                                          |           |           |   Optional  |--------+                                    |
             *            |                                                          |           |           +-------------+        |                 +-----------+      |
             *            |                                                          |      +----+-------+          ||              +-----------------| >Service3 |      |
             *            | RunnableReco   +------------+                            |      | >Plugin1.1 |          ||                                | Optional  |      |
             *            +--------------->|  Service3  |                            |      |  Optional  |       Disabled                             +------+----+      |
             *                             |  Optional  |                            |      +------------+     (InvalidLoop)                                 |           |
             *                             +----+-------+                            |                                                                       |           |
             *                                  |                                    |                                                                  +----+-----+     |
             *                                  |                                    |                                                                  | >Plugin3 |-----+
             *                                  |                                    |                                                                  | Optional | Need Running 
             *                              +---+-------+                            |                                                                  +----------+  
             *                              | Plugin3   |----------------------------+  
             *                              | Optional  |  Running 
             *                              +-----------+   
             * 
             */
            #endregion

            var d = new DiscoveredInfo();
            #region Left part
            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.3", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );
            d.FindService( "Service2.3" ).Generalization = d.FindService( "Service2.2" );
            d.FindService( "Service2.2" ).Generalization = d.FindService( "Service2.1" );
            d.FindService( "Service2.1" ).Generalization = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2.1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2.1" ).Service = d.FindService( "Service2.3" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2.2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2.2" ).Service = d.FindService( "Service2.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service3" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2.3" ), DependencyRequirement.OptionalRecommended );
            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service3" ), DependencyRequirement.RunnableRecommended );
            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service2" ), DependencyRequirement.Running );
            #endregion

            #region Right part (> prefix)
            d.ServiceInfos.Add( new ServiceInfo( ">Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( ">Service1.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( ">Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( ">Service3", d.DefaultAssembly ) );
            d.FindService( ">Service1.1" ).Generalization = d.FindService( ">Service1" );

            d.PluginInfos.Add( new PluginInfo( ">Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( ">Plugin1" ).Service = d.FindService( ">Service1" );
            d.PluginInfos.Add( new PluginInfo( ">Plugin1bis", d.DefaultAssembly ) );
            d.FindPlugin( ">Plugin1bis" ).Service = d.FindService( ">Service1" );
            d.PluginInfos.Add( new PluginInfo( ">Plugin1.1", d.DefaultAssembly ) );
            d.FindPlugin( ">Plugin1.1" ).Service = d.FindService( ">Service1.1" );
            d.PluginInfos.Add( new PluginInfo( ">Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( ">Plugin2" ).Service = d.FindService( ">Service2" );
            d.PluginInfos.Add( new PluginInfo( ">Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( ">Plugin3" ).Service = d.FindService( ">Service3" );

            d.FindPlugin( ">Plugin1" ).AddServiceReference( d.FindService( ">Service2" ), DependencyRequirement.Runnable );
            d.FindPlugin( ">Plugin1bis" ).AddServiceReference( d.FindService( ">Service3" ), DependencyRequirement.Running );
            d.FindPlugin( ">Plugin2" ).AddServiceReference( d.FindService( ">Service1.1" ), DependencyRequirement.Running );
            d.FindPlugin( ">Plugin3" ).AddServiceReference( d.FindService( ">Service2" ), DependencyRequirement.Running );

            #endregion

            d.FindService( ">Service1" ).Generalization = d.FindService( "Service2" );

            YodiiEngine engine = new YodiiEngine( new BuggyYodiiEngineHostMock() );
            engine.SetDiscoveredInfo( d );

            return engine;
        }
        

        [Test]
        public void simple_loop_start_stress_test()
        {
            StartAllStartable( BuildSimpleLoop( ConfigurationStatus.Optional, DependencyRequirement.Runnable, StartDependencyImpact.Unknown ) );
            StartAllStartable( BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.Unknown ) );
            StartAllStartable( BuildSimpleLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.StartRecommended ) );
        }

        [Test]
        public void more_complex_loop_start_stress_test()
        {
            foreach( ConfigurationStatus c in typeof( ConfigurationStatus ).GetEnumValues() )
            {
                for( int i = 0; i < 16; ++i )
                {
                    StartDependencyImpact impact = (StartDependencyImpact)(i << 1);
                    using( TestHelper.ConsoleMonitor.OpenInfo().Send( "Stress test for: {0} - {1}", c, impact ) )
                    {
                        StartAllStartable( BuildMoreComplexLoop( c, impact ) );
                    }
                }
            }
        }

        static void StartAllStartable( YodiiEngine engine, int round = 10, [CallerMemberName]string caller = "" )
        {
            StartAllStartable( new Random().Next(), engine, round, caller );
        }

        static void StartAllStartable( int seed, YodiiEngine engine, int round = 10, [CallerMemberName]string caller = "" )
        {
            Random r = new Random( seed );
            using( TestHelper.ConsoleMonitor.OpenInfo().Send( "StartAllStartable for {1} - Seed = {0}", seed, caller ) )
            {
                engine.FullStartAndStop( ( e, res ) =>
                {
                    res.CheckSuccess();
                    var sP = e.LiveInfo.Plugins.Where( p => p.Capability.CanStart ).ToArray();
                    var sS = e.LiveInfo.Services.Where( p => p.Capability.CanStart ).ToArray();
                    int nbStartable = sP.Length + sS.Length;
                    int count = nbStartable * round;
                    while( --count > 0 )
                    {
                        int i = r.Next( nbStartable );
                        ILiveYodiiItem item = i < sP.Length ? sP[i] : (ILiveYodiiItem)sS[i - sP.Length];
                        var impact = (StartDependencyImpact)(r.Next( 16 ) << 1);
                        if( item.Capability.CanStartWith( impact ) ) e.Start( item, impact );
                    }
                } );
            }
        }

    }
}
