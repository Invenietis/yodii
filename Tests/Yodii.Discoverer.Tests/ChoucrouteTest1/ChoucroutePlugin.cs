using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    public class ChoucroutePlugin : YodiiPluginBase, IChoucrouteService
    {
        IService<IAnotherService> _serviceOpt;

        public ChoucroutePlugin( IOptionalService<IAnotherService> s, Microsoft.SqlServer.Server.SqlContext context )
        {
            _serviceOpt = s;
        }

        void IChoucrouteService.DoSomething()
        {
            Debug.WriteLine( "Done" );
        }
    }
}