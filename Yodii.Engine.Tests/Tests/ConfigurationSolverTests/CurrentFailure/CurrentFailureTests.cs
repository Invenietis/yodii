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
            d.ServiceInfos.Add(new ServiceInfo("Service1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service1.1", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service2", d.DefaultAssembly));
            d.ServiceInfos.Add(new ServiceInfo("Service3", d.DefaultAssembly));
            d.FindService("Service1.1").Generalization = d.FindService("Service1");

            d.PluginInfos.Add(new PluginInfo("Plugin1", d.DefaultAssembly));
            d.FindPlugin("Plugin1").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1bis", d.DefaultAssembly));
            d.FindPlugin("Plugin1bis").Service = d.FindService("Service1");
            d.PluginInfos.Add(new PluginInfo("Plugin1.1", d.DefaultAssembly));
            d.FindPlugin("Plugin1.1").Service = d.FindService("Service1.1");
            d.PluginInfos.Add(new PluginInfo("Plugin2", d.DefaultAssembly));
            d.FindPlugin("Plugin2").Service = d.FindService("Service2");
            d.PluginInfos.Add(new PluginInfo("Plugin3", d.DefaultAssembly));
            d.FindPlugin("Plugin3").Service = d.FindService("Service3");

            d.FindPlugin("Plugin1").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Runnable);
            d.FindPlugin("Plugin1bis").AddServiceReference(d.FindService("Service3"), DependencyRequirement.Running);
            d.FindPlugin("Plugin2").AddServiceReference(d.FindService("Service1.1"), DependencyRequirement.Running);
            d.FindPlugin("Plugin3").AddServiceReference(d.FindService("Service2"), DependencyRequirement.Running);

            YodiiEngine engine = new YodiiEngine(new YodiiEngineHostMock());
            engine.SetDiscoveredInfo(d);



            var result = engine.Start();
            engine.FullStaticResolutionOnly(res =>
            {
                res.CheckSuccess();
                res.CheckPluginsDisabled("Plugin1,Plugin1bis");
                res.CheckAllPluginsRunnable("Plugin2,Plugin1.1,Plugin3");
                System.Diagnostics.Debug.WriteLine(res.StaticSolvedConfiguration.FindPlugin("Plugin2").FinalConfigSolvedStatus.ToString());
                res.CheckAllServicesRunnable("Service1,Service1.1,Service2,Service3");
            });
        }
    }
}
