using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company3 : IYodiiPlugin
    {
        IMarketPlaceService _serviceRef1;
        IDeliveryService _serviceRef2;

        [DependencyRequirementAttribute( DependencyRequirement.Running, "ServiceRef1" )]
        [DependencyRequirementAttribute( DependencyRequirement.Running, "ServiceRef2" )]
        public Company3( IMarketPlaceService ServiceRef1, IDeliveryService ServiceRef2 )
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
