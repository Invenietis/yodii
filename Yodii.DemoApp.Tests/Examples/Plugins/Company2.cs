using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : IYodiiPlugin
    {
        IMarketPlaceService _serviceRef1;
        IDeliveryService _serviceRef2;

        public Company2()
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
