using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class TestRootSubBPluginAlernate : TrackedPluginBase, ITestRootSubBService
    {
        public TestRootSubBPluginAlernate( ITrackerService t )
            : base( t )
        {
        }
    }


}