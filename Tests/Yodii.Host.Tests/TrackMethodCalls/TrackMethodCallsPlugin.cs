using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public class TrackMethodCallsPlugin : YodiiPluginBase, ITrackMethodCallsPluginService
    {
        IOptionalService<ITrackerService> _serviceOpt;

         List<string> _calledMethods;

        public TrackMethodCallsPlugin( IOptionalService<ITrackerService> s )
        {
            _calledMethods = new List<string>();
            _serviceOpt = s;
            _serviceOpt.ServiceStatusChanged += _serviceOpt_ServiceStatusChanged;
            _calledMethods.Add( "Constructor" );
        }

        public List<string> CalledMethods 
        { 
            get { return _calledMethods; } 
        }

        void _serviceOpt_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            _calledMethods.Add( "_serviceOpt_ServiceStatusChanged - IAnotherService.Status = " + _serviceOpt.Status );
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            _calledMethods.Add( "Setup - IAnotherService.Status = " + _serviceOpt.Status );
        }

        protected override void PluginStart( IStartContext c )
        {
            _calledMethods.Add( "Start - IAnotherService.Status = " + _serviceOpt.Status );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _calledMethods.Add( "Stop - IAnotherService.Status = " + _serviceOpt.Status );
        }

        protected override void PluginStop( IStopContext c )
        {
            _calledMethods.Add( "Teardown - IAnotherService.Status = " + _serviceOpt.Status );
        }

        void ITrackMethodCallsPluginService.DoSomething()
        {
            _calledMethods.Add( "DoSomething - IAnotherService.Status = " + _serviceOpt.Status );
        }
    }
}