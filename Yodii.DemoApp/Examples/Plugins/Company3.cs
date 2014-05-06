using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company3 : IYodiiPlugin
    {
        IMarketPlaceService _serviceRef1;
        IDeliveryService _serviceRef2;

        public Company3()
        {
        }

        public IMarketPlaceService ServiceRef1
        {
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            get { return _serviceRef1; }
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            set { _serviceRef1 = value; }
        }

        public IDeliveryService ServiceRef2
        {
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            get { return _serviceRef2; }
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            set { _serviceRef2 = value; }
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
