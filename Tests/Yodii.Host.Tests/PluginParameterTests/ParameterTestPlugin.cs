using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Host.Tests.PluginParameterTests
{
    public class MyCustomClass
    {
        public string TestParameter { get; set; }
    }

    public class ParameterTestPlugin : YodiiPluginBase
    {
        MyCustomClass _testClass;

        public ParameterTestPlugin( MyCustomClass testClass, IChoucrouteService service1, IRunnableService<IChoucrouteService2> service2 )
        {
            _testClass = testClass;
            _testClass.TestParameter = "Hello world";
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            _testClass.TestParameter = "Hello world";
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _testClass.TestParameter = "Hello world";
        }

        protected override void PluginStart( IStartContext c )
        {
            _testClass.TestParameter = "Hello world";
        }

        protected override void PluginStop( IStopContext c )
        {
            _testClass.TestParameter = "Hello world";
        }
    }
}
