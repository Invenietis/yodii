using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public interface ITestRootSubBSubService : ITestRootSubBService
    {
    }

    public class TestRootSubBSubPlugin : TrackedPluginBase, ITestRootSubBSubService
    {
        public TestRootSubBSubPlugin( ITrackerService t )
            : base( t )
        {
        }
    }

    public class TestRootSubBSubPluginAlernate : TrackedPluginBase, ITestRootSubBSubService, IDisposable
    {
        public TestRootSubBSubPluginAlernate( ITrackerService t )
            : base( t )
        {
        }

        void IDisposable.Dispose()
        {
            Assert.Throws<ServiceCallBlockedException>( () => Tracker.AddEntry( this, "Dispose" ) );
        }
    }


}