using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ConsoleDemo.DummyYodiiItems
{
    public class MyPlugin3 : IYodiiPlugin, IMyService2
    {
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
