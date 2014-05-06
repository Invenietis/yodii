using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    [Plugin( "dddddddddddddddddddddddddddddddd", PublicName = "Client1" )]
    public class Client1 : IYodiiPlugin
    {
        private IMarketPlaceService _service;

        public Client1()
        {
        }

        public IMarketPlaceService Service
        {
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            get { return _service; }
            [DependencyRequirementAttribute( DependencyRequirement.Running )]
            set { _service = value; }
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
