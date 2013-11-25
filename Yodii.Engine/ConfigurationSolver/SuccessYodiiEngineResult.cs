using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class SuccessYodiiEngineResult : IYodiiEngineResult
    {
        #region IYodiiEngineResult Members

        public bool Success
        {
            get { throw new NotImplementedException(); }
        }

        public IConfigurationManagerFailureResult ConfigurationManagerFailureResult
        {
            get { throw new NotImplementedException(); }
        }

        public IStaticFailureResult StaticFailureResult
        {
            get { throw new NotImplementedException(); }
        }

        public IDynamicFailureResult HostFailureResult
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IPluginInfo> PluginCulprits
        {
            get { throw new NotImplementedException(); }
        }

        public IReadOnlyList<IServiceInfo> ServiceCulprits
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
