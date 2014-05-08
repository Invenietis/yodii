using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : IYodiiPlugin
    {
        IMarketPlaceService _serviceRef1;
        IDeliveryService _serviceRef2;

        [DependencyRequirementAttribute( DependencyRequirement.Running, "ServiceRef1" )]
        [DependencyRequirementAttribute( DependencyRequirement.Running, "ServiceRef2" )]
        public Company2( IMarketPlaceService ServiceRef1, IDeliveryService ServiceRef2 )
        {
            _serviceRef1 = ServiceRef1;
            _serviceRef2 = ServiceRef2;
        }

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            throw new NotImplementedException();
        }

        void IYodiiPlugin.Start()
        {
            throw new NotImplementedException();
        }

        void IYodiiPlugin.Teardown()
        {
            throw new NotImplementedException();
        }

        void IYodiiPlugin.Stop()
        {
            throw new NotImplementedException();
        }
    }
}
