using System;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public class Plugin1Bis : IYodiiPlugin
    {
        IService<IService2> _service;

        public Plugin1Bis( IRunnableService<IService2> service )
        {
            _service = service;
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
