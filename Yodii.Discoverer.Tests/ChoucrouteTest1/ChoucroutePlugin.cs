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
        IChoucrouteServiceRef _service;

        [DependencyRequirementAttribute( DependencyRequirement.Running, "serviceRef" )]
        public ChoucroutePlugin( IChoucrouteServiceRef serviceRef) 
        {
            _service = serviceRef;
        }

        public ChoucroutePlugin()
        {
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