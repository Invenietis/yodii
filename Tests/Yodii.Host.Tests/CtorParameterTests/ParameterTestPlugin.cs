using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    public class MyCustomClass
    {
        public string TestParameter { get; set; }
    }

    public class ParameterTestPlugin : YodiiPluginBase
    {
        readonly ITrackMethodCallsPluginService _tracker;
        readonly MyCustomClass _testClass;

        public ParameterTestPlugin( MyCustomClass testClass, ITrackMethodCallsPluginService tracker, IRunnableService<IFailureTransitionPluginService> otherService )
        {
            _tracker = tracker;
            _testClass = testClass;
            _testClass.TestParameter = "Ctor";
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.DoSomething() );
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.DoSomething() );
            _testClass.TestParameter = "PreStart";
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _tracker.DoSomething();
            _testClass.TestParameter = "PreStop";
        }

        protected override void PluginStart( IStartContext c )
        {
            _tracker.DoSomething();
            _testClass.TestParameter = "Start";
        }

        protected override void PluginStop( IStopContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.DoSomething() );
            _testClass.TestParameter = "Stop";
        }
    }
}
