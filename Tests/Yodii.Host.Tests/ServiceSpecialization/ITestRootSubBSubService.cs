using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class TestRootSubBSubPluginAlernate : TrackedPluginBase, ITestRootSubBSubService
    {
        public TestRootSubBSubPluginAlernate( ITrackerService t )
            : base( t )
        {
        }
    }


}