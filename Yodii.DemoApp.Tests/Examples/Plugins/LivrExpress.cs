using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : IYodiiPlugin, IDeliveryService
    {
        ICarRepairService _serviceRef1;
        IOutSourcingService _serviceRef2;

        public LivrExpress()
        { 
        }

        public ICarRepairService ServiceRef1
        {
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            get { return _serviceRef1; }
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            set { _serviceRef1 = value; }
        }

        public IOutSourcingService ServiceRef2
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
