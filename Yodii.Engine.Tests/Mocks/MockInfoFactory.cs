using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model.CoreModel;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    internal class MockInfoFactory
    {
        readonly List<PluginInfo> _pluginInfos;
        readonly List<ServiceInfo> _serviceInfos;

        internal MockInfoFactory()
        {
            _pluginInfos = new List<PluginInfo>();
            _serviceInfos = new List<ServiceInfo>();
            IAssemblyInfo executingAssemblyInfo = AssemblyInfoHelper.ExecutingAssemblyInfo;

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
             * +----+-----+
             * |PluginAx-1|
             * +----------+
             */

            ServiceInfo serviceA = new ServiceInfo( "ServiceA", executingAssemblyInfo );
            ServiceInfo serviceB = new ServiceInfo( "ServiceB", executingAssemblyInfo );
            ServiceInfo serviceAx = new ServiceInfo( "ServiceAx", executingAssemblyInfo, serviceA );

            _serviceInfos.Add( serviceA );
            _serviceInfos.Add( serviceB );
            _serviceInfos.Add( serviceAx );

            PluginInfo pluginA1 = new PluginInfo( Guid.NewGuid(), "PluginA-1", executingAssemblyInfo, serviceA );
            serviceA.BindPlugin( pluginA1 );

            PluginInfo pluginA2 = new PluginInfo( Guid.NewGuid(), "PluginA-2", executingAssemblyInfo, serviceA );
            serviceA.BindPlugin( pluginA2 );
            IServiceReferenceInfo A2NeedsServiceBRunningReference = new MockServiceReferenceInfo( pluginA2, serviceB, RunningRequirement.Running );
            pluginA2.BindServiceRequirement( A2NeedsServiceBRunningReference );

            PluginInfo pluginAx1 = new PluginInfo( Guid.NewGuid(), "PluginAx-1", executingAssemblyInfo, serviceAx );
            serviceAx.BindPlugin( pluginAx1 );

            PluginInfo pluginB1 = new PluginInfo( Guid.NewGuid(), "PluginB-1", executingAssemblyInfo, serviceB );
            serviceB.BindPlugin( pluginB1 );

            _pluginInfos.Add( pluginA1 );
            _pluginInfos.Add( pluginA2 );
            _pluginInfos.Add( pluginAx1 );
            _pluginInfos.Add( pluginB1 );
        }

        public IReadOnlyList<IPluginInfo> Plugins
        {
            get
            {
                return _pluginInfos.AsReadOnlyList();
            }
        }

        public IReadOnlyList<IServiceInfo> Services
        {
            get
            {
                return _serviceInfos.AsReadOnlyList();
            }
        }
    }
}
