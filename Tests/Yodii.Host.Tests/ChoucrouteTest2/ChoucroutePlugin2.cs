using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public class ChoucroutePlugin2 : YodiiPluginBase, IChoucrouteService2
    {
        IOptionalService<IAnotherService4> _serviceOpt;

         List<string> _calledMethods;
         public List<string> CalledMethods { get { return _calledMethods; } }

        public ChoucroutePlugin2( IOptionalService<IAnotherService4> s)
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
        protected override void PluginPreStart( IPreStartContext c )
        {
            _calledMethods.Add( "Setup - IAnotherService = " + _serviceOpt.Status );
            throw new InvalidOperationException();
            base.PluginPreStart( c );
        }
        protected override void PluginStart( IStartContext c )
        {
            _calledMethods.Add( "Start - IAnotherService = " + _serviceOpt.Status );
            base.PluginStart( c );
        }
        protected override void PluginPreStop( IPreStopContext c )
        {
            _calledMethods.Add( "Stop - IAnotherService = " + _serviceOpt.Status );
            base.PluginPreStop( c );
        }
        protected override void PluginStop( IStopContext c )
        {
            _calledMethods.Add( "Teardown - IAnotherService = " + _serviceOpt.Status );
            base.PluginStop( c );
        }

        void IChoucrouteService2.DoSomething()
        {
            _calledMethods.Add( "DoSomething - IAnotherService = " + _serviceOpt.Status );
        }
    }
}