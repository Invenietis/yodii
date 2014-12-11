using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public interface ITestRootSubBService : ITestRootService
    {
    }

    public class TestRootSubBPlugin : TrackedPluginBase, ITestRootSubBService
    {
        public TestRootSubBPlugin( ITrackerService t )
            : base( t )
        {
        }
    }

    public class TestRootSubBPluginAlternate : TrackedPluginBase, ITestRootSubBService, IDisposable
    {
        public TestRootSubBPluginAlternate( ITrackerService t )
            : base( t )
        {
        }

        void IDisposable.Dispose()
        {
            Assert.Throws<ServiceCallBlockedException>( () => Tracker.AddEntry( this, "Dispose" ) );
        }
    }


}