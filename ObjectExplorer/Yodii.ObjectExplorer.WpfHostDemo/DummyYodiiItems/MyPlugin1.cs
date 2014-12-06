using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.DummyItems
{
    public class MyPlugin1 : YodiiPluginBase, IMyService1
    {
        public MyPlugin1( IRunnableService<IMyService2> service2 )
        {

        }

        #region IYodiiPlugin Members

        #endregion
    }
}
