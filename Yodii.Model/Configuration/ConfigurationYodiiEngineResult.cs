using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    class ConfigurationYodiiEngineResult : IYodiiEngineResult
    {
        IConfigurationManagerFailureResult _configurationManagerFailureResult;

        /// <summary>
        /// Construct an ConfigurationYodiiEngineResult without errors.
        /// </summary>
        /// <param name="configurationManagerFailureResult"></param>
        /// <remarks>Use this constructor if there is an error.</remarks>
        internal ConfigurationYodiiEngineResult()
        {
        }

        /// <summary>
        /// Construct an ConfigurationYodiiEngineResult with an error's description.
        /// </summary>
        /// <param name="configurationManagerFailureResult"></param>
        /// <remarks>Use this constructor if there is an error.</remarks>
        internal ConfigurationYodiiEngineResult( ConfigurationManagerFailureResult configurationManagerFailureResult )
        {
            Debug.Assert( configurationManagerFailureResult == null );
            if( configurationManagerFailureResult.Success )
            {
                _configurationManagerFailureResult = configurationManagerFailureResult;
            }
        }

        #region IYodiiEngineResult Members

        public bool StaticResolutionSuccess
        {
            get { return _configurationManagerFailureResult != null; }
        }

        public IConfigurationManagerFailureResult ConfigurationManagerFailureResult
        {
            get { return _configurationManagerFailureResult; }
        }

        public IStaticFailureResult StaticFailureResult
        {
            get { return null; ; }
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
