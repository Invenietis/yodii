using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public class ChoucroutePlugin : IChoucrouteService, IYodiiPlugin
    {
        IOptionalService<IAnotherService> _serviceOpt;

        public static List<string> CalledMethods = new List<string>();

        public ChoucroutePlugin( IOptionalService<IAnotherService> s)
        {
            _serviceOpt = s;
            _serviceOpt.ServiceStatusChanged += _serviceOpt_ServiceStatusChanged;
            CalledMethods.Add( "Constructor" );
        }

        void _serviceOpt_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            CalledMethods.Add( "_serviceOpt_ServiceStatusChanged - IAnotherService = " + _serviceOpt.Status );
        }

        public bool Setup( PluginSetupInfo info )
        {
            CalledMethods.Add( "Setup - IAnotherService = " + _serviceOpt.Status );
            return true;
        }

        public void Start()
        {
            CalledMethods.Add( "Start - IAnotherService = " + _serviceOpt.Status );
        }

        public void Teardown()
        {
            CalledMethods.Add( "Teardown - IAnotherService = " + _serviceOpt.Status );
        }

        public void Stop()
        {
            CalledMethods.Add( "Stop - IAnotherService = " + _serviceOpt.Status );
        }

        void IChoucrouteService.DoSomething()
        {
            CalledMethods.Add( "DoSomething - IAnotherService = " + _serviceOpt.Status );
        }
    }
}