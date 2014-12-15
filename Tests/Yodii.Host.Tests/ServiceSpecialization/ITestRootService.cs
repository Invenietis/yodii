using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public interface ITestRootService : IYodiiService
    {
    }

    public class TestRootRootPlugin : TrackedPluginBase, ITestRootService
    {
        public TestRootRootPlugin( ITrackerService t )
            : base( t )
        {
        }
    }

    public class TestRootPluginAlternate : TrackedPluginBase, ITestRootService, IDisposable
    {
        public TestRootPluginAlternate( ITrackerService t )
            : base( t )
        {
        }

        void IDisposable.Dispose()
        {
            Assert.Throws<ServiceCallBlockedException>( () => Tracker.AddEntry( this, "Dispose" ) );
        }
    }

}
