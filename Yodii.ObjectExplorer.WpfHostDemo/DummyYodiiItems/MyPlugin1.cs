using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.DummyItems
{
    public class MyPlugin1 : IMyService1, IYodiiPlugin
    {
        public MyPlugin1( IRunnableService<IMyService2> service2 )
        {

        }

        #region IYodiiPlugin Members

        public bool Setup( PluginSetupInfo info )
        {
            return true;
        }

        public void Start()
        {
        }

        public void Teardown()
        {
        }

        public void Stop()
        {
        }
        #endregion
    }
}
