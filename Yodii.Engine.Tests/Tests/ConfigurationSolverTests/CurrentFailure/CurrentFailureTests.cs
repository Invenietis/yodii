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
        public void RunningServiceWithPlugin()
        {
            #region graph
            /**
             *                  +--------+
             *      +---------->|   S1   |
             *      |           |        |
             *      |           +---+----+       
             *      |               |            
             *      |               |                     
             *      |           +---+-----+              
             *  +---+-----+     |   P     |  
             *  |  S.1.1  |     |         |  
             *  | Running |     +---------+  
             *  +----+----+   
             *       |
             *   +---+-----+              
             *   |   P1    |  
             *   |         |  
             *   +---------+        
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("S1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("S1.1", d.DefaultAssembly));
            d.FindService("S1.1").Generalization = d.FindService("S1");

            d.PluginInfos.Add(new PluginInfo("P", d.DefaultAssembly));
            d.FindPlugin("P").Service = d.FindService("S1");
            d.PluginInfos.Add(new PluginInfo("P1", d.DefaultAssembly));
            d.FindPlugin("P1").Service = d.FindService("S1.1");

            YodiiEngine engine = new YodiiEngine(new YodiiEngineHostMock());
            engine.SetDiscoveredInfo(d);

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add("S1.1", ConfigurationStatus.Running);

            var result = engine.Start();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("P");
            });
        }
        [Test]
        public void RunningServiceWithRunningSiblingPlugin()
        {
            #region graph
            /**
             *                  +--------+
             *      +---------->|   S1   |
             *      |           |        |
             *      |           +---+----+       
             *      |               |            
             *      |               |                     
             *      |           +---+-----+              
             *  +---+-----+     |   P     |  
             *  |  S.1.1  |     | Running |  
             *  | Running |     +---------+  
             *  +----+----+   
             *       |
             *   +---+-----+              
             *   |   P1    |  
             *   |         |  
             *   +---------+        
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("S1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("S1.1", d.DefaultAssembly));
            d.FindService("S1.1").Generalization = d.FindService("S1");

            d.PluginInfos.Add(new PluginInfo("P", d.DefaultAssembly));
            d.FindPlugin("P").Service = d.FindService("S1");
            d.PluginInfos.Add(new PluginInfo("P1", d.DefaultAssembly));
            d.FindPlugin("P1").Service = d.FindService("S1.1");

            YodiiEngine engine = new YodiiEngine(new YodiiEngineHostMock());
            engine.SetDiscoveredInfo(d);

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add("S1.1", ConfigurationStatus.Running);
            cl.Items.Add("P", ConfigurationStatus.Running);

            var result = engine.Start();
            Assert.That(result.Success, Is.False);
        }


        [Test]
        public void InvalidLoop()
        {
            #region graph
        /**
             *                  +--------+                              +--------+
             *      +---------->|Service1+-------+   *----------------->|Service2|
             *      |           |Optional|       |   | Need Running     |Optional|   
             *      |           +---+----+       |   |                  +---+----+
             *      |                        +---+-----+                    |
             *      |                        |Plugin1  |                    |
             *      |                        |Optional |                    |
             *  +---+------+                 +---+-----+                    |
             *  |Service1.1|                                                |
             *  |Optional  |-----------------+                              |
             *  +----+-----+                 |                          +---+-----+
             *       |                       ---------------------------|Plugin2  |
             *       |                                  Need Running    |Optional |
             *       |                                                  +---------+
             *       |
             *       |      
             *       |      
             *  +---+-------+
             *  |Plugin1.1  |
             *  |Optional   |
             *  +-----------+
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");
            d.PluginInfos.Add(new PluginInfo("Plugin2", d.DefaultAssembly));
            d.FindPlugin("Plugin2").Service = d.FindService("Service2");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);
            d.FindPlugin("Plugin2").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new YodiiEngineHostMock());
            engine.SetDiscoveredInfo(d);



            var result = engine.Start();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1");
                res.CheckAllPluginsRunnable("Plugin2,Plugin1.1");
                System.Diagnostics.Debug.WriteLine(res.StaticSolvedConfiguration.FindPlugin("Plugin2").FinalConfigSolvedStatus.ToString());
                res.CheckAllServicesRunnable("Service1,Service1.1,Service2");
            });
        }
    }
}
