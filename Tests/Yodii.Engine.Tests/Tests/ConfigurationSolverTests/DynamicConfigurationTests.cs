#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Tests\ConfigurationSolverTests\DynamicConfigurationTests.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Engine.Tests.Mocks;
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

            StaticConfigurationTests.CreateValidCommonReferences3().FullStartAndStop( ( engine, res ) =>
            {
                engine.StartPlugin( "Plugin4" ).CheckSuccess();
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1|Plugin2, Plugin5|Plugin6" );

                engine.StartPlugin( "Plugin1" ).CheckSuccess();
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.StartPlugin( "Plugin2" ).CheckSuccess();
                engine.CheckAllPluginsRunning( "Plugin2, Plugin3, Plugin6, Plugin7" );

                engine.StopPlugin( "Plugin7" ).CheckSuccess();
                engine.CheckAllPluginsRunning( "Plugin1, Plugin5, Plugin4, Plugin8" );
            } );
        }

        [Test]
        public void CommonReferences3a()
        {
            #region graph
            /*
            *                  +--------+                            +--------+
            *      +-----------|Service1|                            |Service2|---------------+
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
            *       |                   |           |Service3|    |       |                   +--------+                 |          
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

            StaticConfigurationTests.CreateValidCommonReferences3().FullStartAndStop( ( engine, res ) =>
            {
                engine.StopPlugin( "Plugin7" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin7, Plugin3, Plugin2, Plugin6" );
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.StartPlugin( "Plugin4" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin7, Plugin3, Plugin2, Plugin6" );
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.StartPlugin( "Plugin1" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin7, Plugin3, Plugin2, Plugin6" );
                engine.CheckAllPluginsRunning( "Plugin4, Plugin8, Plugin1, Plugin5" );

                engine.StartPlugin( "Plugin2" ).CheckSuccess();
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

            StaticConfigurationTests.CreateValidCommonReferences4().FullStartAndStop( ( engine, res ) =>
                {
                    engine.StartPlugin( "Plugin4", StartDependencyImpact.Minimal );
                    engine.StartPlugin( "Plugin1", StartDependencyImpact.Minimal );
                } );
        }


        [Test]
        public void ValidRunnableReferences()
        {
            // file://Graphs/ValidRunnableReferences.png
            // E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png
            // file://E:\Dev\Yodii\Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png
            // file://Yodii.Engine.Tests\ConfigurationSolverTests\Graphs\ValidRunnableReferences.png

            StaticConfigurationTests.CreateValidRunnableReferences().FullStartAndStop( ( engine, res ) =>
            {
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8, Plugin9" );

                engine.StopPlugin( "Plugin5" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5, Plugin6, Plugin7, Plugin8, Plugin9" );

                engine.StartPlugin( "Plugin5" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin6, Plugin7, Plugin8, Plugin9" );
                engine.CheckAllPluginsRunning( "Plugin5 " );

                engine.StartPlugin( "Plugin2" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin3, Plugin4, Plugin6, Plugin7, Plugin8, Plugin9" );
                engine.CheckAllPluginsRunning( "Plugin2, Plugin5" );

                //engine.LiveInfo.FindPlugin( "Plugin2" ).Start( StartDependencyImpact.StartRecommended );
                //engine.CheckAllPluginsStopped( "Plugin1, Plugin3, Plugin4, Plugin6, Plugin7, Plugin9" );
                //engine.CheckAllPluginsRunning( "Plugin2, Plugin5, Plugin8" );

                engine.StopPlugin( "Plugin2" ).CheckSuccess();
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin6, Plugin7, Plugin8, Plugin9" );
                engine.CheckAllPluginsRunning( "Plugin5 " );
            } );
        }

        [Test]
        public void ValidOnlyOneRunnableReference()
        {
            StaticConfigurationTests.CreateValidOnlyOneRunnableReference().FullStartAndStop( ( engine, res ) =>
            {
                engine.CheckAllServicesStopped( "Service1, Service1.1, Service1.2, Service1.3, Service2, Service2.1, Service2.2" );
                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5" );

                engine.StopPlugin( "Plugin1" ).CheckSuccess();

                engine.CheckAllPluginsStopped( "Plugin1, Plugin2, Plugin3, Plugin4, Plugin5" );
                engine.CheckAllServicesStopped( "Service1, Service1.1, Service1.2, Service1.3, Service2, Service2.1, Service2.2" );

                engine.StartPlugin( "Plugin1" ).CheckSuccess();

                engine.CheckAllServicesStopped( "Service1.2, Service1.3, Service2, Service2.1, Service2.2" );
                engine.CheckAllServicesRunning( "Service1, Service1.1" );
                engine.CheckAllPluginsStopped( "Plugin2, Plugin3, Plugin4, Plugin5" );

                engine.StartPlugin( "Plugin1" ).CheckSuccess();
                engine.StartPlugin( "Plugin2" ).CheckSuccess();
                engine.StartService( "Service1" ).CheckSuccess();
                engine.StopPlugin( "Plugin1" ).CheckSuccess();

                engine.StartPlugin( "Plugin5" ).CheckSuccess();
                engine.StopService( "Service1" ).CheckSuccess();

                engine.StartService( "Service1.1" ).CheckSuccess();
                engine.StartService( "Service1.2" ).CheckSuccess();
                engine.StartService( "Service1" ).CheckSuccess();
                engine.StartService( "Service2.2" ).CheckSuccess();
                engine.StopPlugin( "Plugin1" ).CheckSuccess();
                engine.StopPlugin( "Plugin2" ).CheckSuccess();
                engine.StopService( "Service1.3" ).CheckSuccess();
                engine.StopService( "Service2" ).CheckSuccess();
                engine.StopPlugin( "Plugin3" ).CheckSuccess();
                engine.StopService( "Service2.1" ).CheckSuccess();
                engine.StopService( "Service1.3" ).CheckSuccess();
                engine.StopPlugin( "Plugin5" ).CheckSuccess();
                engine.StartService( "Service1.1" ).CheckSuccess();
                engine.StartPlugin( "Plugin3" ).CheckSuccess();
                engine.StartPlugin( "Plugin4" ).CheckSuccess();
                engine.StartService( "Service1.3" ).CheckSuccess();
                engine.StartPlugin( "Plugin5" ).CheckSuccess();

                engine.StartService( "Service1" ).CheckSuccess();
                engine.StartService( "Service1.1" ).CheckSuccess();
                engine.StartService( "Service1.2" ).CheckSuccess();
                engine.StartPlugin( "Plugin1" ).CheckSuccess();
                engine.StartPlugin( "Plugin2" ).CheckSuccess();
                engine.StopService( "Service1.2" ).CheckSuccess();
                engine.StopService( "Service1" ).CheckSuccess();
                engine.StartService( "Service1.3" ).CheckSuccess();
                engine.StartService( "Service2" ).CheckSuccess();
                engine.StartPlugin( "Plugin3" ).CheckSuccess();
                engine.StartService( "Service2.1" ).CheckSuccess();
                engine.StartService( "Service1.3" ).CheckSuccess();
                engine.StopService( "Service2.2" ).CheckSuccess();
                engine.StopService( "Service1.1" ).CheckSuccess();
                engine.StopPlugin( "Plugin3" ).CheckSuccess();
                engine.StartPlugin( "Plugin5" ).CheckSuccess();
                engine.StartService( "Service2.2" ).CheckSuccess();



                engine.StopPlugin( "Plugin1" ).CheckSuccess();
                engine.StopPlugin( "Plugin2" ).CheckSuccess();
                engine.StopService( "Service1" ).CheckSuccess();
                engine.StopPlugin( "Plugin5" ).CheckSuccess();
                engine.StopService( "Service1.1" ).CheckSuccess();
                engine.StopService( "Service1.2" ).CheckSuccess();
                engine.StopService( "Service1" ).CheckSuccess();
                engine.StopService( "Service2.2" ).CheckSuccess();
                engine.StopService( "Service1.1" ).CheckSuccess();
                engine.StopPlugin( "Plugin3" ).CheckSuccess();
                engine.StopPlugin( "Plugin4" ).CheckSuccess();
                engine.StopService( "Service1.3" ).CheckSuccess();
                engine.StopPlugin( "Plugin5" ).CheckSuccess();

                engine.StopService( "Service1" ).CheckSuccess();
                engine.StopService( "Service1.1" ).CheckSuccess();
                engine.StopService( "Service1.2" ).CheckSuccess();
                engine.StopPlugin( "Plugin1" ).CheckSuccess();
                engine.StopPlugin( "Plugin2" ).CheckSuccess();
                engine.StopService( "Service1.3" ).CheckSuccess();
                engine.StopService( "Service2" ).CheckSuccess();
                engine.StopPlugin( "Plugin3" ).CheckSuccess();
                engine.StopService( "Service2.1" ).CheckSuccess();
                engine.StopService( "Service1.3" ).CheckSuccess();
                engine.StopPlugin( "Plugin5" ).CheckSuccess();
                engine.StopService( "Service2.2" ).CheckSuccess();
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
            *  +---+-----+         |                                 
            *  |Plugin1  |     +---+-----+                           
            *  |Optional |     |Plugin2  |                           
            *  +----+----+     |Optional |-----------------------+ 
            *       |          +---------+                       |
            *       |                                            |
            *       |Optional                                    |
            *       |                              +---------+   |          
            *       |                              |Service2 |<--+         
            *       |       +----------------------|Optional |             
            *       |       |                       +---+----+               
            *       |       |                          |                   
            *       |   +---+-------+             +----+------+            
            *       |   |Service2.1 |             |Service2.2 |        
            *       +-->|Optional   |             |Optional   |        
            *           +-----------+             +-----+-----+            
            *               |                           |            
            *            +--+-----+                  +--+-----+
            *            |Plugin3 |                  |Plugin4 |
            *            |Optional|                  |Optional|
            *            +--------+                  +--------+
            */
            #endregion
            StaticConfigurationTests.CreateValidOptionalReferences().FullStartAndStop( ( engine, res ) =>
            {
                engine.CheckAllPluginsRunning( "Plugin1|Plugin2" );
                engine.CheckServicesRunningLocked( "Service1" );

                engine.StartPlugin( "Plugin1" ).CheckSuccess();
                engine.StartPlugin( "Plugin3" ).CheckSuccess();

                engine.CheckAllPluginsRunning( "Plugin1, Plugin3" );
                engine.CheckAllPluginsStopped( "Plugin2, Plugin4" );
                engine.CheckAllServicesRunning( "Service2.1, Service2" );
                engine.CheckAllServicesStopped( "Service2.2" );

                engine.StopPlugin( "Plugin1" ).CheckSuccess();
                engine.StartService( "Service2.2" ).CheckSuccess();

                engine.CheckAllPluginsRunning( "Plugin2, Plugin4" );
                engine.CheckPluginsStopped( "Plugin1, Plugin3" );
                engine.CheckAllServicesRunning( "Service2.2, Service2" );
                engine.CheckAllServicesStopped( "Service2.1" );
            } );
        }

        [Test]
        public void CodependencyGraphTest()
        {
            StaticConfigurationTests.CreateCoDependencyGraph().FullStartAndStop( ( engine, res ) =>
            {
                engine.StartPlugin( "B", StartDependencyImpact.Minimal ).CheckSuccess();
                engine.CheckAllRunning( "A, B, IA, IB" );
            });
        }

        //[Test]
        //public void ValidOptionalRecommendedReferences()
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
        //    StaticConfigurationTests.CreateValidOptionalRecommendedReferences().FullStart( ( engine, res ) =>
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

        [Test]
        public void RuntimeAssertionTest()
        {
            IYodiiEngineExternal engine = MockXmlUtils.CreateEngineFromXmlResource( "BaseGraph4" );

            engine.StartEngine();

            var rootIService2 = engine.LiveInfo.Services.Where( s => s.FullName == "IService2" ).First();

            var layer = engine.Configuration.Layers.Create( "DefautLayer" );
            var result = layer.Items.Set( "IService2", ConfigurationStatus.Disabled );

            Assert.That( result.Success == false );
        }

        
    }
}
