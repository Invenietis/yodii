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

    public class ParameterTestPlugin : IYodiiPlugin
    {
        MyCustomClass _testClass;
        public ParameterTestPlugin( MyCustomClass testClass, IChoucrouteService service1, IRunnableService<IChoucrouteService2> service2 )
        {
            _testClass = testClass;
            _testClass.TestParameter = "Hello world";
        }
        #region IYodiiPlugin Members

        public bool Setup( PluginSetupInfo info )
        {
            _testClass.TestParameter = "Hello world";
            return true;
        }

        public void Start()
        {
            _testClass.TestParameter = "Hello world";
        }

        public void Teardown()
        {
            _testClass.TestParameter = "Hello world";
        }

        public void Stop()
        {
            _testClass.TestParameter = "Hello world";
        }

        #endregion
    }
}
