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
            get { return true; }
        }

        public IConfigurationFailureResult ConfigurationFailureResult
        {
            get { return null; }
        }

        public IStaticFailureResult StaticFailureResult
        {
            get { return null; }
        }

        public IDynamicFailureResult HostFailureResult
        {
            get { return null; }
        }

        public IReadOnlyList<IPluginInfo> PluginCulprits
        {
            get { return null; }
        }

        public IReadOnlyList<IServiceInfo> ServiceCulprits
        {
            get { return null; }
        }

        #endregion
    }
}
