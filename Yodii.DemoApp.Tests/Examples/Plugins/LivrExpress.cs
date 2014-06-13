using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : IYodiiPlugin, IDeliveryService
    {
        ICarRepairService _serviceRef1;
        IOutSourcingService _serviceRef2;

        [DependencyRequirement( DependencyRequirement.Running, "ServiceRef1" )]
        [DependencyRequirement( DependencyRequirement.Running, "ServiceRef2" )]
        public LivrExpress( ICarRepairService ServiceRef1, IOutSourcingService ServiceRef2 )
        {
            _serviceRef1 = ServiceRef1;
            _serviceRef2 = ServiceRef2;
        }
        
        public bool Setup( PluginSetupInfo info )
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Teardown()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
    }
}
