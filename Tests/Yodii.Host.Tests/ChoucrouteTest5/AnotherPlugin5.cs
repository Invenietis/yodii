﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public class AnotherPlugin5 : IYodiiPlugin
    {
        IRunningService<IChoucrouteService5Generalization> _serviceRunning;

         List<string> _calledMethods;
         public List<string> CalledMethods { get { return _calledMethods; } }

         public AnotherPlugin5( IRunningService<IChoucrouteService5Generalization> s )
        {
            _calledMethods = new List<string>();
            _serviceRunning = s;
            _serviceRunning.ServiceStatusChanged += _serviceOpt_ServiceStatusChanged;
            _calledMethods.Add( "Constructor" );
        }

        void _serviceOpt_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            _calledMethods.Add( "_serviceOpt_ServiceStatusChanged - IAnotherService = " + _serviceRunning.Status );
        }

        public bool Setup( PluginSetupInfo info )
        {
            _calledMethods.Add( "Setup - IAnotherService = " + _serviceRunning.Status );
            return true;
        }

        public void Start()
        {
            _calledMethods.Add( "Start - IAnotherService = " + _serviceRunning.Status );
        }

        public void Teardown()
        {
            _calledMethods.Add( "Teardown - IAnotherService = " + _serviceRunning.Status );
        }

        public void Stop()
        {
            _calledMethods.Add( "Stop - IAnotherService = " + _serviceRunning.Status );
        }

        public void DoSomething()
        {
            _calledMethods.Add( "DoSomething  = " + _serviceRunning.Status );
            _serviceRunning.Service.DoSomethingElse();
        }
    }
}