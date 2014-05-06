using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : IYodiiPlugin
    {
        public MarketPlace()
        { 
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
