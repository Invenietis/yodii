using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public class ChoucroutePlugin : IChoucrouteService, IYodiiPlugin
    {
        IService<IAnotherService> _serviceOpt;

        public ChoucroutePlugin( IOptionalService<IAnotherService> s, Microsoft.SqlServer.Server.SqlContext context )
        {
            _serviceOpt = s;
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

        void IChoucrouteService.DoSomething()
        {
            Debug.WriteLine( "Done" );
        }
    }
}