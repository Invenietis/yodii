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
        readonly string _pluginFullName;

        public ChoucroutePlugin( string pluginFullName ) 
        {
            _pluginFullName = pluginFullName;
        }

        public ChoucroutePlugin()
        {
        }

        public void DoSomething()
        {
            Debug.WriteLine( "DoSomething!" );
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
