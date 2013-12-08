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
        public void ConfigurationSolverCommonReferencesWork2()
        {
            #region graph
            /*
            *                  +--------+                            +--------+
            *      +-----------|Service1+                            |Service2|---------------+
            *      |           |Running |                            |Running |               |
            *      |           +---+----+                            +---+----+               |
            *      |               |                                      |                   |
            *      |               |                                      |                   |
            *      |               |                                      |                   |
            *  +---+-----+         |                                      |                   |
            *  |Plugin1  |     +---+-----+                            +---+-----+         +---+-----+
            *  |Optional |     |Plugin2  |                            |Plugin3  |         |Plugin4  |
            *  +----+----+     |Optional |                            |Optional |         |Optional |
            *       |          +---------+                            +---------+         +-----+---+
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |                                 |                     |
             *      |                   |                                 |                     |
            *       |                   |                                 |                     |
            *       |                   |           +--------+            |                     |
            *       |                   |           |Service3+            |                     |
            *       |       +-----------|-----------|Optional|------------|------+--------------+-----------+
            *       |       |           |           +---+----+            |      |              |           |                
            *       |       |           |               |                 |      |              |           |                
            *       |       |           |               |                 |      |              |           |                
            *       |   +---+-------+   +-------->+-----+-----+           |  +---+-------+      |       +---+-------+        
            *       |   |Service3.1 |             |Service3.2 |           |  |Service3.3 |      |       |Service3.4 |        
            *       +-->|Optional   |             |Optional   |           +->|Optional   |      +------>|Optional   |        
             *          +-----------+             +-----------+              +-----------+              +-----------+        
             *          |           |             |           |              |           |              |           |        
             *          |           |             |           |              |           |              |           |        
             *          |           |             |           |              |           |              |           |        
             *      +---+-----+ +---+-----+   +---+-----+ +---+-----+    +---+-----+ +---+-----+    +---+-----+ +---+-----+  
             *      |Plugin5  | |Plugin6  |   |Plugin7  | |Plugin8  |    |Plugin9  | |Plugin10 |    |Plugin11 | |Plugin12 |  
             *      |Optional | |Optional |   |Optional | |Optional |    |Optional | |Optional |    |Optional | |Optional |  
             *      +---------+ +---------+   +---------+ +---------+    +---------+ +---------+    +---------+ +---------+  
             * 
            */
            #endregion
            DiscoveredInfo info = MockInfoFactory.CreateGraph005b();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.BlockingServices.Count == 1 );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count == 0 );
        }

        [Test]
        public void ConfigurationSolverCommonReferencesWork3()
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
             *          |Plugin6 |                  |Plugin7 |              +--+-----+                  +---+----+
             *          |Optional|                  |Optional|              |Plugin8 |                  |Plugin9 |
             *          +--------+                  +--------+              |Optional|                  |Optional|
             *                                                              +--------+                  +--------+
            */
            #endregion

            DiscoveredInfo info = MockInfoFactory.CreateGraph005c();
            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );

            IConfigurationLayer cl = engine.ConfigurationManager.Layers.Create();
            cl.Items.Add( "Service1", ConfigurationStatus.Running );
            cl.Items.Add( "Service2", ConfigurationStatus.Running );

            ConfigurationSolver cs = new ConfigurationSolver();
            IYodiiEngineResult res = cs.StaticResolution( engine.ConfigurationManager.FinalConfiguration, info );

            Assert.That( res.Success, Is.False );
            Assert.That( res.StaticFailureResult, Is.Not.Null );
            Assert.That( res.StaticFailureResult.BlockingServices.Count == 1 );
            Assert.That( res.StaticFailureResult.BlockingPlugins.Count == 0 );
        }

    }
}
