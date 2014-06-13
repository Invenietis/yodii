using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : IYodiiPlugin
    {
        private IMarketPlaceService _service;

        [DependencyRequirement( DependencyRequirement.Running, "Service" )]
        public Client1( IMarketPlaceService service )
        {
            _service = service;
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
