using System;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public class Plugin1Bis
    {
        IService<IService2> _service;

        public Plugin1Bis( IRunnableService<IService2> service )
        {
            _service = service;
        }
    }
}
