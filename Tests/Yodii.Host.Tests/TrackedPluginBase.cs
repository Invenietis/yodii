using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public abstract class TrackedPluginBase : YodiiPluginBase
    {
        readonly ITrackerService _tracker;

        protected TrackedPluginBase( ITrackerService t )
        {
            _tracker = t;
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.AddEntry( this, "PreStart" ) );
        }

        protected override void PluginStart( IStartContext c )
        {
            _tracker.AddEntry( this, "Start" );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _tracker.AddEntry( this, "PreStop" );
        }

        protected override void PluginStop( IStopContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.AddEntry( this, "Stop" ) );
        }

    }
}
