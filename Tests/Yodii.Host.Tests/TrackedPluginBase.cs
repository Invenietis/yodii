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
        protected readonly ITrackerService Tracker;

        protected TrackedPluginBase( ITrackerService t )
        {
            Tracker = t;
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => Tracker.AddEntry( this, "PreStart" ) );
        }

        protected override void PluginStart( IStartContext c )
        {
            Tracker.AddEntry( this, "Start" );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            Tracker.AddEntry( this, "PreStop" );
        }

        protected override void PluginStop( IStopContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => Tracker.AddEntry( this, "Stop" ) );
        }

    }
}
