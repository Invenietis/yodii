using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Engine.Tests.ConfigurationSolverTests
{
    [TestFixture]
    class DynamicConfigurationTests
    {

        [Test]
        public void CommonReferences3()
        {
            #region graph
            /*
            *                  +--------+                            +--------+
            *      +-----------|Service1+                            |Service2|---------------+
            *      |           |Running |                            |Running |               |      
            *      |           +---+----+                            +----+---+               |      
            *      |               |                                      |                   |      
            *      |               |                                      |                   |      
            *      |               |                                      |                   |      
            *  +---+-----+         |                                      |                   |      
            *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+
            *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  +--------------------+
            *  +----+----+     |Optional |------------------------+   |Optional |         |Optional |                    | 
            *       |          +---------+                        |   +---------+         +---------+                    | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
             *      |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |           +--------+    |       |                                              |          
            *       |                   |           |Service3+    |       |                   +--------+                 |          
            *       |       +-----------|-----------|Optional|    |       |                   |Service4+                 |          
            *       |       |           |           +---+----+    |       |       +-----------|Optional|-------+         |            
            *       |       |           |               |         |       |       |           +---+----+       |         |               
            *       |       |           |               |         |       |       |                            |         |           
            *       |   +---+-------+   |          +----+------+  |       |       |                            |         |           
            *       |   |Service3.1 |   |          |Service3.2 |  |       |    +--+--------+             +-----+-----+   |       
            *       +-->|Optional   |   |          |Optional   |  +-------|--->|Service4.1 |             |Service4.2 |   |       
             *          +-----------+   |          +-----+-----+          |    |Optional   |             |Optional   |<--+       
             *              |           |                |                |    +-----------+             +-----+-----+     
             *              |           |                |                |        |                           |           
             *          +---+-------+   +--------->+-----+-----+          |        |                           |
             *          |Service3.3 |              |Service3.4 |          | +---+-------+              +----+------+  
             *          |Optional   |              |Optional   |          +>|Service4.3 |              |Service4.4 |  
             *          +--+--------+              +-----------+            |Optional   |              |Optional   |  
             *             |                            |                   +--+--------+              +-----------+ 
             *             |                            |                      |                            |
             *             |                            |                      |                            |
             *          +--+-----+                  +---+----+                 |                            |
             *          |Plugin5 |                  |Plugin6 |              +--+-----+                  +---+----+
             *          |Optional|                  |Optional|              |Plugin7 |                  |Plugin8 |
             *          +--------+                  +--------+              |Optional|                  |Optional|
             *                                                              +--------+                  +--------+
            */
            #endregion

            StaticConfigurationTests.CreateValidCommonReferences3().FullStart( ( engine, res ) =>
            {
                engine.LiveInfo.FindPlugin( "Plugin4" ).Start();
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1|Plugin2, Plugin5|Plugin6" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Start();
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.LiveInfo.FindPlugin( "Plugin2" ).Start();
                engine.CheckAllPluginsRunning( "Plugin2, Plugin3, Plugin6, Plugin7" );

                engine.LiveInfo.FindPlugin( "Plugin7" ).Stop();
                engine.CheckAllPluginsRunning( "Plugin1, Plugin5, Plugin4, Plugin8" );
            } );
        }

        [Test]
        public void CommonReferences3a()
        {
            #region graph
            /*
            *                  +--------+                            +--------+
            *      +-----------|Service1+                            |Service2|---------------+
            *      |           |Running |                            |Running |               |      
            *      |           +---+----+                            +----+---+               |      
            *      |               |                                      |                   |      
            *      |               |                                      |                   |      
            *      |               |                                      |                   |      
            *  +---+-----+         |                                      |                   |      
            *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+
            *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  +--------------------+
            *  +----+----+     |Optional |------------------------+   |Optional |         |Optional |                    | 
            *       |          +---------+                        |   +---------+         +---------+                    | 
            *       |                   |                         |       |                                              | 
            *       |                   |                         |       |                                              | 
            *       |                   |           +--------+    |       |                                              |          
            *       |                   |           |Service3+    |       |                   +--------+                 |          
            *       |       +-----------|-----------|Optional|    |       |                   |Service4+                 |          
            *       |       |           |           +---+----+    |       |       +-----------|Optional|-------+         |            
            *       |       |           |               |         |       |       |           +---+----+       |         |               
            *       |       |           |               |         |       |       |                            |         |           
            *       |   +---+-------+   |          +----+------+  |       |       |                            |         |           
            *       |   |Service3.1 |   |          |Service3.2 |  |       |    +--+--------+             +-----+-----+   |       
            *       +-->|Optional   |   |          |Optional   |  +-------|--->|Service4.1 |             |Service4.2 |   |       
             *          +-----------+   |          +-----+-----+          |    |Optional   |             |Optional   |<--+       
             *              |           |                |                |    +-----------+             +-----+-----+     
             *              |           |                |                |        |                           |           
             *          +---+-------+   +--------->+-----+-----+          |        |                           |
             *          |Service3.3 |              |Service3.4 |          | +---+-------+              +----+------+  
             *          |Optional   |              |Optional   |          +>|Service4.3 |              |Service4.4 |  
             *          +--+--------+              +-----------+            |Optional   |              |Optional   |  
             *             |                            |                   +--+--------+              +-----------+ 
             *             |                            |                      |                            |
             *             |                            |                      |                            |
             *          +--+-----+                  +---+----+                 |                            |
             *          |Plugin5 |                  |Plugin6 |              +--+-----+                  +---+----+
             *          |Optional|                  |Optional|              |Plugin7 |                  |Plugin8 |
             *          +--------+                  +--------+              |Optional|                  |Optional|
             *                                                              +--------+                  +--------+
            */
            #endregion

            StaticConfigurationTests.CreateValidCommonReferences3().FullStart( ( engine, res ) =>
            {
                engine.LiveInfo.FindPlugin( "Plugin7" ).Stop();
                engine.CheckAllPluginsStopped( "Plugin7, Plugin3, Plugin2, Plugin6" );
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.LiveInfo.FindPlugin( "Plugin4" ).Start();
                engine.CheckAllPluginsStopped( "Plugin7, Plugin3, Plugin2, Plugin6" );
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Start();
                engine.CheckAllPluginsStopped( "Plugin7, Plugin3, Plugin2, Plugin6" );
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );
                
                engine.LiveInfo.FindPlugin( "Plugin2" ).Start();
            } );
        }

        [Test]
        public void CommonReferences4()
        {
            #region graph
            /*
            *                  +--------+                            
            *      +-----------|Service1+                            
            *      |           |Running |                            
            *      |           +---+----+                            
            *      |               |                                 
            *      |               |                                 
            *      |               |                                 
            *  +---+-----+         |                                 
            *  |Plugin1  |     +---+-----+                           
            *  |Optional |     |Plugin2  |                           
            *  +----+----+     |Optional |-----------------------+ 
            *       |          +---------+                       |
            *       |                                            |
            *       |                                            |
            *       |                                            |
            *       |                                            |
            *       |Runnable                                    |
             *      |                                            |
            *       |                                            |
             *      |                                            |
            *       |                              +---------+   |          
            *       |                              |Service2 |<--+         
            *       |       +----------------------|Optional |             
            *       |       |                       +---+----+               
            *       |       |                          |                       
            *       |       |                          |                   
            *       |   +---+-------+             +----+------+            
            *       |   |Service2.1 |             |Service2.2 |        
            *       +-->|Optional   |             |Optional   |        
            *           +-----------+             +-----+-----+            
            *               |                           |            
            *               |                           |            
            *               |                         +--+-----+
            *               |                         |Plugin4 |
            *            +--+-----+                   |Optional|
            *            |Plugin3 |                   +--------+
            *            |Optional|          
            *            +--------+          
            *                           
            *                           
            *                                                        
            */
            #endregion

            StaticConfigurationTests.CreateValidCommonReferences4().FullStart( (engine,res) =>
                {
                    engine.LiveInfo.FindPlugin( "Plugin4" ).Start( "caller", StartDependencyImpact.Minimal );
                    engine.LiveInfo.FindPlugin( "Plugin1" ).Start( "caller", StartDependencyImpact.Minimal );
                } );
        }


        [Test]
        public void ValidRunnableReferences()
        {
            // file://Graphs/ValidRunnableReferences.png
            // E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png
            // file://E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png
            // file://Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png

            StaticConfigurationTests.CreateValidRunnableReferences().FullStart( ( engine, res ) =>
            {
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8, Plugin9" );
                
                engine.LiveInfo.FindPlugin( "Plugin5" ).Stop();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8, Plugin9" );
                
                engine.LiveInfo.FindPlugin( "Plugin5" ).Start();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin6, Plugin7, Plugin8, Plugin9" );
                engine.CheckAllPluginsRunning( "Plugin5 " );

                engine.LiveInfo.FindPlugin( "Plugin2" ).Start();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin3, Plugin4, Plugin6, Plugin7, Plugin8, Plugin9" );
                engine.CheckAllPluginsRunning( "Plugin2, Plugin5" );

                //engine.LiveInfo.FindPlugin( "Plugin2" ).Start( StartDependencyImpact.StartRecommended );
                //engine.CheckAllPluginsStopped( "Plugin1, Plugin3, Plugin4, Plugin6, Plugin7, Plugin9" );
                //engine.CheckAllPluginsRunning( "Plugin2, Plugin5, Plugin8" );

                engine.LiveInfo.FindPlugin( "Plugin2" ).Stop();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin6, Plugin7, Plugin8, Plugin9" );
                engine.CheckAllPluginsRunning( "Plugin5 " );
            } );
        }

        [Test]
        public void ValidOnlyOneRunnableReference()
        {
            StaticConfigurationTests.CreateValidOnlyOneRunnableReference().FullStart( ( engine, res ) =>
            {
                engine.CheckAllServicesStopped( "Service1, Service1.1, Service1.2, Service1.3, Service2, Service2.1, Service2.2" );
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Stop();

                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5" );
                engine.CheckAllServicesStopped( "Service1, Service1.1, Service1.2, Service1.3, Service2, Service2.1, Service2.2" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Start();
                
                engine.CheckAllServicesStopped( "Service1.2, Service1.3, Service2, Service2.1, Service2.2" );
                engine.CheckAllServicesRunning( "Service1, Service1.1" );
                engine.CheckAllPluginsStopped( "Plugin2, Plugin3, Plugin4, Plugin5" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin2" ).Start();
                engine.LiveInfo.FindService( "Service1" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin1" ).Stop();

                engine.LiveInfo.FindPlugin( "Plugin5" ).Start();
                engine.LiveInfo.FindService( "Service1" ).Stop();

                engine.LiveInfo.FindService( "Service1.1" ).Start();
                engine.LiveInfo.FindService( "Service1.2" ).Start();
                engine.LiveInfo.FindService( "Service1" ).Start();
                engine.LiveInfo.FindService( "Service2.2" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin1" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin2" ).Stop();
                engine.LiveInfo.FindService( "Service1.3" ).Stop();
                engine.LiveInfo.FindService( "Service2" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Stop();
                engine.LiveInfo.FindService( "Service2.1" ).Stop();
                engine.LiveInfo.FindService( "Service1.3" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin5" ).Stop();
                engine.LiveInfo.FindService( "Service1.1" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin4" ).Start();
                engine.LiveInfo.FindService( "Service1.3" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin5" ).Start();

                engine.LiveInfo.FindService( "Service1" ).Start();
                engine.LiveInfo.FindService( "Service1.1" ).Start();
                engine.LiveInfo.FindService( "Service1.2" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin1" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin2" ).Start();
                engine.LiveInfo.FindService( "Service1.2" ).Stop();
                engine.LiveInfo.FindService( "Service1" ).Stop();
                engine.LiveInfo.FindService( "Service1.3" ).Start();
                engine.LiveInfo.FindService( "Service2" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Start();
                engine.LiveInfo.FindService( "Service2.1" ).Start();
                engine.LiveInfo.FindService( "Service1.3" ).Start();
                engine.LiveInfo.FindService( "Service2.2" ).Stop();
                engine.LiveInfo.FindService( "Service1.1" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin5" ).Start();
                engine.LiveInfo.FindService( "Service2.2" ).Start();



                engine.LiveInfo.FindPlugin( "Plugin1" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin2" ).Stop();
                engine.LiveInfo.FindService( "Service1" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin5" ).Stop();
                engine.LiveInfo.FindService( "Service1.1" ).Stop();
                engine.LiveInfo.FindService( "Service1.2" ).Stop();
                engine.LiveInfo.FindService( "Service1" ).Stop();
                engine.LiveInfo.FindService( "Service2.2" ).Stop();
                engine.LiveInfo.FindService( "Service1.1" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin4" ).Stop();
                engine.LiveInfo.FindService( "Service1.3" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin5" ).Stop();

                engine.LiveInfo.FindService( "Service1" ).Stop();
                engine.LiveInfo.FindService( "Service1.1" ).Stop();
                engine.LiveInfo.FindService( "Service1.2" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin1" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin2" ).Stop();
                engine.LiveInfo.FindService( "Service1.3" ).Stop();
                engine.LiveInfo.FindService( "Service2" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Stop();
                engine.LiveInfo.FindService( "Service2.1" ).Stop();
                engine.LiveInfo.FindService( "Service1.3" ).Stop();
                engine.LiveInfo.FindPlugin( "Plugin5" ).Stop();
                engine.LiveInfo.FindService( "Service2.2" ).Stop();
            } );
        }

        [Test]
        public void ValidOptionalReferences()
        {
            #region graph
            /*
            *                  +--------+                            
            *      +-----------|Service1+                            
            *      |           |Running |                            
            *      |           +---+----+                            
            *      |               |                                 
            *      |               |                                 
            *      |               |                                 
            *  +---+-----+         |                                 
            *  |Plugin1  |     +---+-----+                           
            *  |Optional |     |Plugin2  |                           
            *  +----+----+     |Optional |-----------------------+ 
            *       |          +---------+                       |
            *       |                                            |
            *       |                                            |
            *       |                                            |
            *       |                                            |
            *       |Optional                                    |
             *      |                                            |
            *       |                                            |
             *      |                                            |
            *       |                              +---------+   |          
            *       |                              |Service2 |<--+         
            *       |       +----------------------|Optional |             
            *       |       |                       +---+----+               
            *       |       |                          |                       
            *       |       |                          |                   
            *       |   +---+-------+             +----+------+            
            *       |   |Service2.1 |             |Service2.2 |        
            *       +-->|Optional   |             |Optional   |        
            *           +-----------+             +-----+-----+            
            *               |                           |            
            *               |                           |            
            *               |                         +--+-----+
            *               |                         |Plugin4 |
            *            +--+-----+                   |Optional|
            *            |Plugin3 |                   +--------+
            *            |Optional|          
            *            +--------+          
            *                           
            *                           
            *                                                        
            */
            #endregion
            StaticConfigurationTests.CreateValidOptionalReferences().FullStart( ( engine, res ) =>
            {
                engine.CheckAllPluginsRunning( "Plugin1|Plugin2" );
                engine.CheckServicesRunningLocked( "Service1" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Start();
                engine.LiveInfo.FindPlugin( "Plugin3" ).Start();

                engine.CheckAllPluginsRunning( "Plugin1, Plugin3" );
                engine.CheckPluginsStopped( "Plugin2, Plugin4" );
                engine.CheckAllServicesRunning("Service2.1, Service2");
                engine.CheckAllServicesStopped( "Service2.2" );

                engine.LiveInfo.FindPlugin( "Plugin1" ).Stop();
                engine.LiveInfo.FindService( "Service2.2" ).Start();

                engine.CheckAllPluginsRunning( "Plugin2, Plugin4" );
                engine.CheckPluginsStopped( "Plugin1, Plugin3" );
                engine.CheckAllServicesRunning( "Service2.2, Service2" );
                engine.CheckAllServicesStopped( "Service2.1" );
            } );
        }

        //[Test]
        //public void ValidOptionalTryStartReferences()
        //{
        //    #region graph
        //    /*
        //    *                  +--------+                            +--------+
        //    *      +-----------|Service1+                            |Service2|---------------+
        //    *      |           |Running |                            |Running |               |      
        //    *      |           +---+----+                            +----+---+               |      
        //    *      |               |                                      |                   |      
        //    *      |               |                                      |                   |      
        //    *      |               |                                      |                   |      
        //    *  +---+-----+         |                                      |                   |      
        //    *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+
        //    *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  +--------------------+
        //    *  +----+----+     |Optional |------------------------+   |Optional |         |Optional |                    | 
        //    *       |          +---------+                        |   +---------+         +---------+                    | 
        //    *       |                   |                         |       |                                              | 
        //    *       |                   |                         |       |                                              | 
        //    *       |                   |                         |       |                                              | 
        //    *       |                   |                         |       |                                              | 
        //    *       |                   |                         |       |                                              | 
        //     *      |                   |                         |       |                                              | 
        //    *       |                   |                         |       |                                              | 
        //    *       |                   |           +--------+    |       |                                              |          
        //    *       |                   |           |Service3+    |       |                   +--------+                 |          
        //    *       |       +-----------|-----------|Optional|    |       |                   |Service4+                 |          
        //    *       |       |           |           +---+----+    |       |       +-----------|Optional|-------+         |            
        //    *       |       |           |               |         |       |       |           +---+----+       |         |               
        //    *       |       |           |               |         |       |       |                            |         |           
        //    *       |   +---+-------+   |          +----+------+  |       |       |                            |         |           
        //    *       |   |Service3.1 |   |          |Service3.2 |  |       |    +--+--------+             +-----+-----+   |       
        //    *       +-->|Optional   |   |          |Optional   |  +-------|--->|Service4.1 |             |Service4.2 |   |       
        //     *          +-----------+   |          +-----+-----+          |    |Optional   |             |Optional   |<--+       
        //     *              |           |                |                |    +-----------+             +-----+-----+     
        //     *              |           |                |                |        |                           |           
        //     *          +---+-------+   +--------->+-----+-----+          |        |                           |
        //     *          |Service3.3 |              |Service3.4 |          | +---+-------+              +----+------+  
        //     *          |Optional   |              |Optional   |          +>|Service4.3 |              |Service4.4 |  
        //     *          +--+--------+              +-----------+            |Optional   |              |Optional   |  
        //     *             |                            |                   +--+--------+              +-----------+ 
        //     *             |                            |                      |                            |
        //     *             |                            |                      |                            |
        //     *          +--+-----+                  +---+----+                 |                            |
        //     *          |Plugin5 |                  |Plugin6 |              +--+-----+                  +---+----+
        //     *          |Optional|                  |Optional|              |Plugin7 |                  |Plugin8 |
        //     *          +--------+                  +--------+              |Optional|                  |Optional|
        //     *                                                              +--------+                  +--------+
        //    */
        //    #endregion
        //    StaticConfigurationTests.CreateValidOptionalTryStartReferences().FullStart( ( engine, res ) =>
        //    {
        //        engine.CheckAllServicesRunningLocked( "Service1, Service2" );
        //        //After the static resolution and before submitting any yodiiCommands (see TryStarts)

        //        //Let's assume Plugin1 and Plugin 3 are running
        //        if ( engine.LiveInfo.FindPlugin( "Plugin1" ).RunningStatus == RunningStatus.Running && engine.LiveInfo.FindPlugin( "Plugin3" ).RunningStatus == RunningStatus.Running )
        //        {
        //            engine.CheckRunning( "Plugin1, Plugin5, Plugin3, Plugin7" );
        //            engine.CheckServicesRunning( "Service3, Service3.1, Service3.3, Service4.1, Service4.3, Service4" );
        //            engine.CheckServicesStopped( "Service3.2, Service3.4, Service4.2, Service4.4" );
        //            engine.CheckPluginsStopped( "Plugin6, Plugin8, Plugin2, Plugin4" );
        //        }

        //        //Let's assume Plugin1 and Plugin 4 is running
        //        else if ( engine.LiveInfo.FindPlugin( "Plugin1" ).RunningStatus == RunningStatus.Running && engine.LiveInfo.FindPlugin( "Plugin4" ).RunningStatus == RunningStatus.Running )
        //        {
        //            engine.CheckPluginsRunning( "Plugin1, Plugin5, Plugin4, Plugin8" );
        //            engine.CheckServicesRunning( "Service3, Service3.1, Service3.3, Service4, Service4.2, Service4.4" );
        //            engine.CheckServicesStopped( "Service3.2, Service3.4, Service4.1, Service4.3" );
        //            engine.CheckPluginsStopped( "Plugin6, Plugin7, Plugin2, Plugin3" );
        //        }

        //        //Let's assume Plugin2 and Plugin 3 is running
        //        else if ( engine.LiveInfo.FindPlugin( "Plugin2" ).RunningStatus == RunningStatus.Running && engine.LiveInfo.FindPlugin( "Plugin3" ).RunningStatus == RunningStatus.Running )
        //        {
        //            engine.CheckPluginsRunning( "Plugin2, Plugin3, Plugin6, Plugin7" );
        //            engine.CheckServicesRunning( "Service3, Service3.2, Service3.4, Service4, Service4.1, Service4.3" );
        //            engine.CheckServicesStopped( "Service3.1, Service3.3, Service4.2, Service4.4" );
        //            engine.CheckPluginsStopped( "Plugin1, Plugin4, Plugin5, Plugin8" );
        //        }

        //        //Let's assume Plugin 2 and Plugin 4 is running, the system can still be in 2 different states depending on the graph traversal
        //        else if ( engine.LiveInfo.FindPlugin( "Plugin2" ).RunningStatus == RunningStatus.Running && engine.LiveInfo.FindPlugin( "Plugin4" ).RunningStatus == RunningStatus.Running )
        //        {
        //            if ( engine.LiveInfo.FindPlugin( "Plugin7" ).RunningStatus == RunningStatus.Running )
        //            {
        //                engine.CheckPluginsRunning( "Plugin2, Plugin4, Plugin6, Plugin7" );
        //                engine.CheckServicesRunning( "Service3, Service3.2, Service3.4, Service4, Service4.1, Service4.3" );
        //                engine.CheckServicesStopped( "Service3.1, Service3.3, Service4.2, Service4.4" );
        //                engine.CheckPluginsStopped( "Plugin1, Plugin3, Plugin5, Plugin8" );
        //            }
        //            else
        //            {
        //                engine.CheckPluginsRunning( "Plugin2, Plugin4, Plugin6, Plugin8" );
        //                engine.CheckServicesRunning( "Service3, Service3.2, Service3.4, Service4, Service4.2, Service4.4" );
        //                engine.CheckServicesStopped( "Service3.1, Service3.3, Service4.1, Service4.3" );
        //                engine.CheckPluginsStopped( "Plugin1, Plugin3, Plugin5, Plugin7" );
        //            }
        //        }
        //    } );
        //}
    }
}
