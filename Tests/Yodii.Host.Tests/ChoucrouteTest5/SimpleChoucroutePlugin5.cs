using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public class SimpleChoucroutePlugin5 : IChoucrouteService5Generalization, IYodiiPlugin
    {
        IOptionalService<IAnotherService4> _serviceOpt;

         List<string> _calledMethods;
         public List<string> CalledMethods { get { return _calledMethods; } }

        public SimpleChoucroutePlugin5( IOptionalService<IAnotherService4> s)
        {
            _calledMethods = new List<string>();
            _serviceOpt = s;
            _serviceOpt.ServiceStatusChanged += _serviceOpt_ServiceStatusChanged;
            _calledMethods.Add( "Constructor" );
        }

        void _serviceOpt_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            _calledMethods.Add( "_serviceOpt_ServiceStatusChanged - IAnotherService = " + _serviceOpt.Status );
        }

        public bool Setup( PluginSetupInfo info )
        {
            _calledMethods.Add( "Setup - IAnotherService = " + _serviceOpt.Status );
            return true;
        }

        public void Start()
        {
            _calledMethods.Add( "Start - IAnotherService = " + _serviceOpt.Status );
        }

        public void Teardown()
        {
            _calledMethods.Add( "Teardown - IAnotherService = " + _serviceOpt.Status );
        }

        public void Stop()
        {
            _calledMethods.Add( "Stop - IAnotherService = " + _serviceOpt.Status );
        }

        public void DoSomethingElse()
        {
            _calledMethods.Add( "DoSomething - IAnotherService = " + _serviceOpt.Status );
        }
    }
}