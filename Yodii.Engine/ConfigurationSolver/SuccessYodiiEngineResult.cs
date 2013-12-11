using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class SuccessYodiiEngineResult : IYodiiEngineResult
    {
        public static readonly IYodiiEngineResult Default = new SuccessYodiiEngineResult();

        SuccessYodiiEngineResult()
        {
        }

        bool IYodiiEngineResult.Success
        {
            get { return true; }
        }

        IConfigurationFailureResult IYodiiEngineResult.ConfigurationFailureResult
        {
            get { return null; }
        }

        IStaticFailureResult IYodiiEngineResult.StaticFailureResult
        {
            get { return null; }
        }

        IDynamicFailureResult IYodiiEngineResult.HostFailureResult
        {
            get { return null; }
        }

        IReadOnlyList<IPluginInfo> IYodiiEngineResult.PluginCulprits
        {
            get { return CK.Core.CKReadOnlyListEmpty<IPluginInfo>.Empty; }
        }

        IReadOnlyList<IServiceInfo> IYodiiEngineResult.ServiceCulprits
        {
            get { return CK.Core.CKReadOnlyListEmpty<IServiceInfo>.Empty; ; }
        }

    }
}
