using System;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public class Plugin1Bis :  YodiiPluginBase
    {
        IService<ITest3Service2> _service;

        public Plugin1Bis( IRunnableService<ITest3Service2> service )
        {
            _service = service;
        }
    }
}
