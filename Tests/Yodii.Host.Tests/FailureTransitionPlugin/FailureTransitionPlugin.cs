using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    /// <summary>
    /// This plugin can fail its PreStart or PreStop calls depending on FailureTransitionPlugin.CancelPreStart and CancelPreStop static properties.
    /// </summary>
    public class FailureTransitionPluginDisposable : FailureTransitionPlugin, IDisposable
    {
        public FailureTransitionPluginDisposable( IAnotherService s, ITrackerService tracker ) : base( s, tracker ) {}
        public void Dispose() {}
    }

    /// <summary>
    /// This plugin can fail its PreStart or PreStop calls depending on CancelPreStart and CancelPreStop static properties.
    /// </summary>
    public class FailureTransitionPlugin : YodiiPluginBase, IFailureTransitionPluginService
    {
        readonly ITrackerService _tracker;
        readonly IAnotherService _another;

        public static bool CancelPreStart = false;
        public static bool CancelPreStop = false;

        public FailureTransitionPlugin( IAnotherService s, ITrackerService tracker )
        {
            _tracker = tracker;
            _another = s;
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.AddEntry( this, "PreStart" ) );
            if( CancelPreStart ) c.Cancel( "Canceled!" );
        }

        protected override void PluginStart( IStartContext c )
        {
            _tracker.AddEntry( this, "Start" );
            Assert.That( c.CancellingPreStop, Is.EqualTo( CancelPreStop ) );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _tracker.AddEntry( this, "PreStop" );
            if( CancelPreStop ) c.Cancel( "Canceled!" );
        }

        protected override void PluginStop( IStopContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.AddEntry( this, "Stop" ) );
            Assert.That( c.CancellingPreStart, Is.EqualTo( CancelPreStart ) );
        }

        void IFailureTransitionPluginService.DoSomething()
        {
            _tracker.AddEntry( this, "DoSomething" );
        }
    }
}