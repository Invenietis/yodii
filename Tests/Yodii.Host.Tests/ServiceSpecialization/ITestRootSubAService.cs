using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public interface ITestRootSubAService : ITestRootService
    {
    }

    public class TestRootSubAPlugin : TrackedPluginBase, ITestRootSubAService
    {
        public TestRootSubAPlugin( ITrackerService t )
            : base( t )
        {
        }
    }

    public class TestRootSubAPluginAlternate : TrackedPluginBase, ITestRootSubAService, IDisposable
    {
        public TestRootSubAPluginAlternate( ITrackerService t )
            : base( t )
        {
        }

        void IDisposable.Dispose()
        {
            Assert.Throws<ServiceCallBlockedException>( () => Tracker.AddEntry( this, "Dispose" ) );
        }
    }

}