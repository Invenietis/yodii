using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Tests.TestYodiiObjects
{
    public class SubServicePlugin : YodiiPluginBase, ISubService
    {
        #region ISubService Members

        public void HelloWorld2()
        {
            throw new NotImplementedException();
        }

        #endregion

        #region IMyYodiiService Members

        public void HelloWorld()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
