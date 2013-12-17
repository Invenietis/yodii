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

            StaticConfigurationTests.CreateValidCommonReferences3().FullStart( (engine,res) =>
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

    }
}
