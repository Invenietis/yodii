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
        public void RunningServiceWithNoPlugin()
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
             *                 
             */
            #endregion

            var d = new DiscoveredInfo();
            d.ServiceInfos.Add( new ServiceInfo( "S1", d.DefaultAssembly ) );
            d.ServiceInfos.Add( new ServiceInfo( "S1.1", d.DefaultAssembly ) );
            d.FindService( "S1.1" ).Generalization = d.FindService( "S1" );

            d.PluginInfos.Add( new PluginInfo( "P", d.DefaultAssembly ) );
            d.FindPlugin( "P" ).Service = d.FindService( "S1" );

            YodiiEngine engine = new YodiiEngine( new YodiiEngineHostMock() );
            engine.SetDiscoveredInfo( d );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "S1.1", ConfigurationStatus.Running );

            var result = engine.Start();
            Assert.That( result.Success, Is.False );
        }


    }
}
