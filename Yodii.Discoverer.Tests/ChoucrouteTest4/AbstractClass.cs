using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public abstract class AbstractClass : IYodiiPlugin
    {

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
