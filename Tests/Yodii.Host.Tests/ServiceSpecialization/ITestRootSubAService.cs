using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
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

    public class TestRootSubAPluginAlternate : TrackedPluginBase, ITestRootSubAService
    {
        public TestRootSubAPluginAlternate( ITrackerService t )
            : base( t )
        {
        }
    }

}