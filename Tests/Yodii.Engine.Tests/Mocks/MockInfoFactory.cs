using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    internal static class MockInfoFactory
    {
        public static DiscoveredInfo CreateGraph001()
        {
            /**
             *                  +--------+                              +--------+
             *      +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *      |           |        |       |   | Need Running     |        |   
             *      |           +---+----+       |   |                  +---+----+
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *      |               |            |   |                      |
             *  +---+-----+         |        +---+---*-+                    |
             *  |ServiceAx|     +---+-----+  |PluginA-2|                +---+-----+
             *  |         |     |PluginA-1|  |         |                |PluginB-1|
             *  +----+----+     |         |  +---------+                |         |
             *       |          +---------+                             +---------+
             *       |
             *  +----+-----+
             *  |PluginAx-1|
             *  |          |
             *  +----------+
             */

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "ServiceA", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceB", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceAx", d.DefaultAssembly ) );
            d.FindService( "ServiceAx" ).Generalization = d.FindService( "ServiceA" );

            d.PluginInfos.Add( new PluginInfo( "PluginA-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginA-2", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginAx-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginB-1", d.DefaultAssembly ) );

            d.FindPlugin( "PluginA-1" ).Service = d.FindService( "ServiceA" );
            d.FindPlugin( "PluginA-2" ).Service = d.FindService( "ServiceA" );
            d.FindPlugin( "PluginAx-1" ).Service = d.FindService( "ServiceAx" );
            d.FindPlugin( "PluginB-1" ).Service = d.FindService( "ServiceB" );

            d.FindPlugin( "PluginA-2" ).AddServiceReference( d.FindService( "ServiceB" ), DependencyRequirement.Running );

            return d;
        }

        public static DiscoveredInfo CreateGraph002()
        {
            /**
             *                 +--------+                              +--------+
             *     +---------->|ServiceA+-------+   *----------------->|ServiceB|
             *     |           +---+----+       |   | Need Running     +---+----+
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             *     |               |            |   |                      |
             * +---+-----+     +---+-----+  +---+---*-+                +---+-----+
             * |ServiceAx|     |PluginA-1|  |PluginA-2|                |PluginB-1|
             * +----+----+     +---------+  +---------+                +---------+
             *      |
             *      |
             *      |--------|
             *      |   +----+-----+
             *      |   |PluginAx-1|
             *      |   +----------+
             *      |        
             *      |        
             *  +---+------+  
             *  |ServiceAxx|  
             *  +----+-----+  
             *       |      
             *       |      
             *       |      
             *       |      
             *  +---+-------+
             *  |PluginAxx-1|
             *  +-----------+
             */

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "ServiceA", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceB", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceAx", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceAxx", d.DefaultAssembly ) );
            d.FindService( "ServiceAx" ).Generalization = d.FindService( "ServiceA" );
            d.FindService( "ServiceAxx" ).Generalization = d.FindService( "ServiceAx" );

            d.PluginInfos.Add( new PluginInfo( "PluginA-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginA-2", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginAx-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginAxx-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginB-1", d.DefaultAssembly ) );

            d.FindPlugin( "PluginA-1" ).Service = d.FindService( "ServiceA" );
            d.FindPlugin( "PluginA-2" ).Service = d.FindService( "ServiceA" );
            d.FindPlugin( "PluginAx-1" ).Service = d.FindService( "ServiceAx" );
            d.FindPlugin( "PluginAxx-1" ).Service = d.FindService( "ServiceAxx" );
            d.FindPlugin( "PluginB-1" ).Service = d.FindService( "ServiceB" );

            d.FindPlugin( "PluginA-2" ).AddServiceReference( d.FindService( "ServiceB" ), DependencyRequirement.Running );

            return d;
        }

        public static DiscoveredInfo CreateGraph003()
        {
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |        |       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |        +---+---*-+
             *  +---+-----+  |PluginA-2|
             *  |PluginA-1|  |         |
             *  |         |  +---------+
             *  +---------+
             */

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "ServiceA", d.DefaultAssembly ) );

            d.PluginInfos.Add( new PluginInfo( "PluginA-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginA-2", d.DefaultAssembly ) );

            d.FindPlugin( "PluginA-1" ).Service = d.FindService( "ServiceA" );
            d.FindPlugin( "PluginA-2" ).Service = d.FindService( "ServiceA" );

            return d;
        }

        public static DiscoveredInfo CreateGraph004()
        {
            /**
             *  +--------+
             *  |ServiceA+ ------+
             *  |        |       |
             *  +---+----+       |
             *      |            |
             *      |            |
             *      |            |
             *      |         +---+------+
             *  +---+------+  |ServiceAx2|
             *  |ServiceAx1|  |          |
             *  |          |  +----------+
             *  +----------+      |       
             *      |             |       
             *      |             |       
             *      |             |       
             *      |          +---+-------+
             *  +---+-------+  |PluginAx2-1|
             *  |PluginAx1-1|  |           |
             *  |           |  +-----------+
             *  +-----------+
             */

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "ServiceA", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceAx1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "ServiceAx2", d.DefaultAssembly ) );
            d.FindService( "ServiceAx1" ).Generalization = d.FindService( "ServiceA" );
            d.FindService( "ServiceAx2" ).Generalization = d.FindService( "ServiceA" );

            d.PluginInfos.Add( new PluginInfo( "PluginAx1-1", d.DefaultAssembly ) );
            d.PluginInfos.Add( new PluginInfo( "PluginAx2-2", d.DefaultAssembly ) );

            d.FindPlugin( "PluginAx1-1" ).Service = d.FindService( "ServiceAx1" );
            d.FindPlugin( "PluginAx2-2" ).Service = d.FindService( "ServiceAx2" );

            return d;
        }

        public static DiscoveredInfo CreateGraph005()
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
            *       +-->|Optional   |             |Optional   |           +->|Optional   |<-----+       |Optional   |        
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

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.1", d.DefaultAssembly ) );
            d.FindService( "Service3.1" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.2", d.DefaultAssembly ) );
            d.FindService( "Service3.2" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.3", d.DefaultAssembly ) );
            d.FindService( "Service3.3" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.4", d.DefaultAssembly ) );
            d.FindService( "Service3.4" ).Generalization = d.FindService( "Service3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service3.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin6", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin6" ).Service = d.FindService( "Service3.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin7", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin7" ).Service = d.FindService( "Service3.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin8", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin8" ).Service = d.FindService( "Service3.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin9", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin9" ).Service = d.FindService( "Service3.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin10", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin10" ).Service = d.FindService( "Service3.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin11", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin11" ).Service = d.FindService( "Service3.4" );

            d.PluginInfos.Add( new PluginInfo( "Plugin12", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin12" ).Service = d.FindService( "Service3.4" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service3.1" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service3.2" ), DependencyRequirement.Running );

            d.FindPlugin( "Plugin4" ).AddServiceReference( d.FindService( "Service3.3" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service3.3" ), DependencyRequirement.Running );

            return d;
        }

        public static DiscoveredInfo CreateGraph005b()
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

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.1", d.DefaultAssembly ) );
            d.FindService( "Service3.1" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.2", d.DefaultAssembly ) );
            d.FindService( "Service3.2" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.3", d.DefaultAssembly ) );
            d.FindService( "Service3.3" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.4", d.DefaultAssembly ) );
            d.FindService( "Service3.4" ).Generalization = d.FindService( "Service3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service3.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin6", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin6" ).Service = d.FindService( "Service3.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin7", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin7" ).Service = d.FindService( "Service3.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin8", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin8" ).Service = d.FindService( "Service3.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin9", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin9" ).Service = d.FindService( "Service3.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin10", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin10" ).Service = d.FindService( "Service3.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin11", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin11" ).Service = d.FindService( "Service3.4" );

            d.PluginInfos.Add( new PluginInfo( "Plugin12", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin12" ).Service = d.FindService( "Service3.4" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service3.1" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service3.2" ), DependencyRequirement.Running );

            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service3.3" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin4" ).AddServiceReference( d.FindService( "Service3.4" ), DependencyRequirement.Running );

            return d;
        }

        public static DiscoveredInfo CreateGraph005c()
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

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service4", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3.3", d.DefaultAssembly ) );
            d.FindService( "Service3.1" ).Generalization = d.FindService( "Service3" );
            d.FindService( "Service3.3" ).Generalization = d.FindService( "Service3.1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3.4", d.DefaultAssembly ) );
            d.FindService( "Service3.2" ).Generalization = d.FindService( "Service3" );
            d.FindService( "Service3.4" ).Generalization = d.FindService( "Service3.2" );

            d.ServiceInfos.Add( new ServiceInfo( "Service4.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service4.3", d.DefaultAssembly ) );
            d.FindService( "Service4.1" ).Generalization = d.FindService( "Service4" );
            d.FindService( "Service4.3" ).Generalization = d.FindService( "Service4.1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service4.2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service4.4", d.DefaultAssembly ) );
            d.FindService( "Service4.2" ).Generalization = d.FindService( "Service4" );
            d.FindService( "Service4.4" ).Generalization = d.FindService( "Service4.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service3.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin6", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin6" ).Service = d.FindService( "Service3.4" );

            d.PluginInfos.Add( new PluginInfo( "Plugin7", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin7" ).Service = d.FindService( "Service4.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin8", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin8" ).Service = d.FindService( "Service4.4" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service3.1" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service3.4" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service4.1" ), DependencyRequirement.Running );

            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service4.3" ), DependencyRequirement.Running );
            d.FindPlugin( "Plugin4" ).AddServiceReference( d.FindService( "Service4.2" ), DependencyRequirement.Running );

            return d;
        }

        internal static IDiscoveredInfo CreateGraph005d()
        {
            DiscoveredInfo d = new DiscoveredInfo();

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
            *       |                              |Service2 |   |         
            *       |       +----------------------|Optional |   |          
            *       |       |                       +---+----+   |            
            *       |       |                          |         |              
            *       |       |                          |         |          
            *       |   +---+-------+             +----+------+  |          
            *       |   |Service2.1 |             |Service2.2 |<-+      
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

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.1", d.DefaultAssembly ) );
            d.FindService( "Service2.1" ).Generalization = d.FindService( "Service2" );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.2", d.DefaultAssembly ) );
            d.FindService( "Service2.2" ).Generalization = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2.1" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service2.2" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2.2" );

            return d;
        }

        internal static IDiscoveredInfo CreateGraph005e()
        {
            DiscoveredInfo d = new DiscoveredInfo();

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
            *       |                              |Service2 |   |         
            *       |       +----------------------|Optional |   |          
            *       |       |                       +---+----+   |            
            *       |       |                          |         |              
            *       |       |                          |         |          
            *       |   +---+-------+             +----+------+  |          
            *       |   |Service2.1 |             |Service2.2 |<-+      
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

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.1", d.DefaultAssembly ) );
            d.FindService( "Service2.1" ).Generalization = d.FindService( "Service2" );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.2", d.DefaultAssembly ) );
            d.FindService( "Service2.2" ).Generalization = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2.1" ), DependencyRequirement.Optional );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service2.2" ), DependencyRequirement.Optional );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2.2" );

            return d;
        }

        internal static IDiscoveredInfo CreateGraph005f()
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

            var d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service4", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3.3", d.DefaultAssembly ) );
            d.FindService( "Service3.1" ).Generalization = d.FindService( "Service3" );
            d.FindService( "Service3.3" ).Generalization = d.FindService( "Service3.1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3.4", d.DefaultAssembly ) );
            d.FindService( "Service3.2" ).Generalization = d.FindService( "Service3" );
            d.FindService( "Service3.4" ).Generalization = d.FindService( "Service3.2" );

            d.ServiceInfos.Add( new ServiceInfo( "Service4.1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service4.3", d.DefaultAssembly ) );
            d.FindService( "Service4.1" ).Generalization = d.FindService( "Service4" );
            d.FindService( "Service4.3" ).Generalization = d.FindService( "Service4.1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service4.2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service4.4", d.DefaultAssembly ) );
            d.FindService( "Service4.2" ).Generalization = d.FindService( "Service4" );
            d.FindService( "Service4.4" ).Generalization = d.FindService( "Service4.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service3.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin6", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin6" ).Service = d.FindService( "Service3.4" );

            d.PluginInfos.Add( new PluginInfo( "Plugin7", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin7" ).Service = d.FindService( "Service4.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin8", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin8" ).Service = d.FindService( "Service4.4" );

            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service3.1" ), DependencyRequirement.OptionalRecommended );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service3.4" ), DependencyRequirement.OptionalRecommended );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service4.1" ), DependencyRequirement.OptionalRecommended );

            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service4.3" ), DependencyRequirement.OptionalRecommended );
            d.FindPlugin( "Plugin4" ).AddServiceReference( d.FindService( "Service4.2" ), DependencyRequirement.OptionalRecommended );

            return d;
        }

        internal static IDiscoveredInfo CreateGraph006()
        {
            DiscoveredInfo d = new DiscoveredInfo();

            #region graph
            //See PNG for better viewing
            #endregion

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service3", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service3.1", d.DefaultAssembly ) );
            d.FindService( "Service3.1" ).Generalization = d.FindService( "Service3" );
            d.ServiceInfos.Add( new ServiceInfo( "Service3.2", d.DefaultAssembly ) );
            d.FindService( "Service3.2" ).Generalization = d.FindService( "Service3" );
            d.ServiceInfos.Add( new ServiceInfo( "Service3.3", d.DefaultAssembly ) );
            d.FindService( "Service3.3" ).Generalization = d.FindService( "Service3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service4", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service4.1", d.DefaultAssembly ) );
            d.FindService( "Service4.1" ).Generalization = d.FindService( "Service4" );
            d.ServiceInfos.Add( new ServiceInfo( "Service4.2", d.DefaultAssembly ) );
            d.FindService( "Service4.2" ).Generalization = d.FindService( "Service4" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service4.1" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin2" ).AddServiceReference( d.FindService( "Service3.3" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2" );
            d.FindPlugin( "Plugin3" ).AddServiceReference( d.FindService( "Service3.2" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2" );
            d.FindPlugin( "Plugin4" ).AddServiceReference( d.FindService( "Service3.1" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service4.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin6", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin6" ).Service = d.FindService( "Service4.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin7", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin7" ).Service = d.FindService( "Service3.1" );
            d.FindPlugin( "Plugin7" ).AddServiceReference( d.FindService( "Service4.1" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin8", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin8" ).Service = d.FindService( "Service3.3" );
            d.FindPlugin( "Plugin8" ).AddServiceReference( d.FindService( "Service1" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin9", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin9" ).Service = d.FindService( "Service3.2" );
            d.FindPlugin( "Plugin9" ).AddServiceReference( d.FindService( "Service4.2" ), DependencyRequirement.Runnable );

            return d;
        }

        internal static IDiscoveredInfo CreateGraph007()
        {
            DiscoveredInfo d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.1", d.DefaultAssembly ) );
            d.FindService( "Service1.1" ).Generalization = d.FindService( "Service1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.2", d.DefaultAssembly ) );
            d.FindService( "Service1.2" ).Generalization = d.FindService( "Service1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.3", d.DefaultAssembly ) );
            d.FindService( "Service1.3" ).Generalization = d.FindService( "Service1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.1", d.DefaultAssembly ) );
            d.FindService( "Service2.1" ).Generalization = d.FindService( "Service2" );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.2", d.DefaultAssembly ) );
            d.FindService( "Service2.2" ).Generalization = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service1.1" );
            d.FindPlugin( "Plugin1" ).AddServiceReference( d.FindService( "Service2.1" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service1.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service1.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service2.2" );

            return d;
        }

        internal static IDiscoveredInfo CreateGraph008()
        {
            DiscoveredInfo d = new DiscoveredInfo();

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.1", d.DefaultAssembly ) );
            d.FindService( "Service1.1" ).Generalization = d.FindService( "Service1" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.2", d.DefaultAssembly ) );
            d.FindService( "Service1.2" ).Generalization = d.FindService( "Service1" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.3", d.DefaultAssembly ) );
            d.FindService( "Service1.3" ).Generalization = d.FindService( "Service1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.1.1", d.DefaultAssembly ) );
            d.FindService( "Service1.1.1" ).Generalization = d.FindService( "Service1.1" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.1.2", d.DefaultAssembly ) );
            d.FindService( "Service1.1.2" ).Generalization = d.FindService( "Service1.1" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.1.3", d.DefaultAssembly ) );
            d.FindService( "Service1.1.3" ).Generalization = d.FindService( "Service1.1" );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.2.1", d.DefaultAssembly ) );
            d.FindService( "Service1.2.1" ).Generalization = d.FindService( "Service1.2" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.2.2", d.DefaultAssembly ) );
            d.FindService( "Service1.2.2" ).Generalization = d.FindService( "Service1.2" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.2.3", d.DefaultAssembly ) );
            d.FindService( "Service1.2.3" ).Generalization = d.FindService( "Service1.2" );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.3.1", d.DefaultAssembly ) );
            d.FindService( "Service1.3.1" ).Generalization = d.FindService( "Service1.3" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.3.2", d.DefaultAssembly ) );
            d.FindService( "Service1.3.2" ).Generalization = d.FindService( "Service1.3" );
            d.ServiceInfos.Add( new ServiceInfo( "Service1.3.3", d.DefaultAssembly ) );
            d.FindService( "Service1.3.3" ).Generalization = d.FindService( "Service1.3" );

            d.ServiceInfos.Add( new ServiceInfo( "Service2.1", d.DefaultAssembly ) );
            d.FindService( "Service2.1" ).Generalization = d.FindService( "Service2" );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.2", d.DefaultAssembly ) );
            d.FindService( "Service2.2" ).Generalization = d.FindService( "Service2" );
            d.ServiceInfos.Add( new ServiceInfo( "Service2.3", d.DefaultAssembly ) );
            d.FindService( "Service2.3" ).Generalization = d.FindService( "Service2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1" ).Service = d.FindService( "Service2.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2" ).Service = d.FindService( "Service2.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin3", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin3" ).Service = d.FindService( "Service2.2" );
            d.PluginInfos.Add( new PluginInfo( "Plugin4", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin4" ).Service = d.FindService( "Service2.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin5", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin5" ).Service = d.FindService( "Service2.3" );
            d.PluginInfos.Add( new PluginInfo( "Plugin6", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin6" ).Service = d.FindService( "Service2.3" );
            d.FindPlugin( "Plugin6" ).AddServiceReference( d.FindService( "Service1.2.3" ), DependencyRequirement.Runnable );

            d.PluginInfos.Add( new PluginInfo( "Plugin7", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin7" ).Service = d.FindService( "Service1.1.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin8", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin8" ).Service = d.FindService( "Service1.1.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin9", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin9" ).Service = d.FindService( "Service1.1.2" );
            d.PluginInfos.Add( new PluginInfo( "Plugin10", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin10" ).Service = d.FindService( "Service1.1.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin11", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin11" ).Service = d.FindService( "Service1.1.3" );
            d.PluginInfos.Add( new PluginInfo( "Plugin12", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin12" ).Service = d.FindService( "Service1.1.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin13", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin13" ).Service = d.FindService( "Service1.2.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin14", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin14" ).Service = d.FindService( "Service1.2.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin15", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin15" ).Service = d.FindService( "Service1.2.2" );
            d.PluginInfos.Add( new PluginInfo( "Plugin16", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin16" ).Service = d.FindService( "Service1.2.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin17", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin17" ).Service = d.FindService( "Service1.2.3" );
            d.FindPlugin( "Plugin17" ).AddServiceReference( d.FindService( "Service2.2" ), DependencyRequirement.RunnableRecommended );

            d.PluginInfos.Add( new PluginInfo( "Plugin18", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin18" ).Service = d.FindService( "Service1.2.3" );

            d.PluginInfos.Add( new PluginInfo( "Plugin19", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin19" ).Service = d.FindService( "Service1.3.1" );
            d.PluginInfos.Add( new PluginInfo( "Plugin20", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin20" ).Service = d.FindService( "Service1.3.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin21", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin21" ).Service = d.FindService( "Service1.3.2" );
            d.PluginInfos.Add( new PluginInfo( "Plugin22", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin22" ).Service = d.FindService( "Service1.3.2" );

            d.PluginInfos.Add( new PluginInfo( "Plugin23", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin23" ).Service = d.FindService( "Service1.3.3" );
            d.PluginInfos.Add( new PluginInfo( "Plugin24", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin24" ).Service = d.FindService( "Service1.3.3" );

            return d;
        }

        internal static IDiscoveredInfo CreateGraphDynamicInvalidLoop()
        {
            DiscoveredInfo d = new DiscoveredInfo();

            #region graph
            /*
            *                  +------------+  
            *      +-----------|  Service1  |  
            *      |           |  Optional  |  
            *      |           +------+-----+  
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

            d.ServiceInfos.Add( new ServiceInfo( "Service1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "Service2", d.DefaultAssembly ) );

            d.ServiceInfos.Add( new ServiceInfo( "Service1.1", d.DefaultAssembly ) );
            d.FindService( "Service1.1" ).Generalization = d.FindService( "Service1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1.1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1.1" ).Service = d.FindService( "Service1.1" );

            d.PluginInfos.Add( new PluginInfo( "Plugin1.2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin1.2" ).Service = d.FindService( "Service1" );
            d.FindPlugin( "Plugin1.2" ).AddServiceReference( d.FindService( "Service2" ), DependencyRequirement.Running );

            d.PluginInfos.Add( new PluginInfo( "Plugin2.1", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2.1" ).Service = d.FindService( "Service2" );
            d.FindPlugin( "Plugin2.1" ).AddServiceReference( d.FindService( "Service1.1" ), DependencyRequirement.Running );

            d.PluginInfos.Add( new PluginInfo( "Plugin2.2", d.DefaultAssembly ) );
            d.FindPlugin( "Plugin2.2" ).Service = d.FindService( "Service2" );

            return d;
        }
    }
}
