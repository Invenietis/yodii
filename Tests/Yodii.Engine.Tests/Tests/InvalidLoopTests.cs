using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Engine.Tests.Mocks;
using Yodii.Model;

namespace Yodii.Engine.Tests.Tests
{
    [TestFixture]
    public class InvalidLoopTests
    {
        [Test]
        public void running_plugin_blocks_if_dependency_must_start()
        {
            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.Running, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" )
                .CheckItem( "Plugin1", p => p == null, "When static resoltuion fails, LiveInfo is not available." );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.RunnableRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.Runnable, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );
        }
        
        [Test]
        public void running_plugin_runs_and_disables_the_reference()
        {

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );
            

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.Optional, StartDependencyImpact.TryStartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );

            BuildLoop( ConfigurationStatus.Running, DependencyRequirement.OptionalRecommended, StartDependencyImpact.TryFullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.RunningLocked )
                .CheckRunningStatus( "Plugin2", RunningStatus.Disabled );
        }

        [Test]
        public void runnable_plugin_blocks_if_dependency_must_start()
        {
            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.Running, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" )
                .CheckItem( "Plugin1", p => p == null, "When static resoltuion fails, LiveInfo is not available." );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckFailure()
                .CheckAllBlockingPluginsAre( "Plugin1" );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.Optional, StartDependencyImpact.FullStart )
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
            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.RunnableRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.Runnable, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckFailure()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.Optional, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.OptionalRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.Optional, StartDependencyImpact.TryStartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Runnable, DependencyRequirement.OptionalRecommended, StartDependencyImpact.TryFullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );
        }

        [Test]
        public void optional_plugin_does_not_block_but_is_disabled_if_dependency_must_start()
        {
            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.Running, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.RunnableRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.RunnableRecommended, StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.StartRecommendedAndStopOptionalAndRunnable )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.Runnable, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.Optional, StartDependencyImpact.FullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Disabled )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped );
        }

        [Test]
        public void optional_plugin_can_run_but_starting_the_dependency_stops_it()
        {
            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.Optional, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.Minimal )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.Optional, StartDependencyImpact.TryStartRecommended )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStart == false );

            BuildLoop( ConfigurationStatus.Optional, DependencyRequirement.OptionalRecommended, StartDependencyImpact.TryFullStart )
                .StartEngine()
                .CheckSuccess()
                .CheckRunningStatus( "Plugin1", RunningStatus.Stopped )
                .CheckRunningStatus( "Plugin2", RunningStatus.Stopped )
                .CheckItem( "Plugin1", p => p.Capability.CanStart )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithFullStop )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommended == false )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStopOptionalAndRunnable )
                .CheckItem( "Plugin1", p => p.Capability.CanStartWithStartRecommendedAndStopOptionalAndRunnable == false )
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
        YodiiEngine BuildLoop( ConfigurationStatus plugin1Config, DependencyRequirement fromPlugin1ToService2, StartDependencyImpact plugin1Impact )
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
            engine.Configuration.Layers.Create().Items.Set( "Plugin1", plugin1Config, plugin1Impact );

            return engine;
        }

    }
}
