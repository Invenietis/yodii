using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    public class StaticFailureResult : IStaticFailureResult
    {
        List<IDynamicSolvedPlugin> _blockingPlugins;
        List<IDynamicSolvedService> _blockingServices;

        internal StaticFailureResult()
        {

        }

        public List<IDynamicSolvedPlugin> BlockingPlugins
        {
            get { return _blockingPlugins; }
        }
        public List<IDynamicSolvedService> BlockingServices
        {
            get { return _blockingServices; }
        }
    }
}
