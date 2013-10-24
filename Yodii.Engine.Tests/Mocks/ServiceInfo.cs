using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;
using Yodii.Model.CoreModel;
using Yodii.Model.ConfigurationSolver;

namespace Yodii.Engine.Tests.Mocks
{
    public class ServiceInfo : DiscoveredInfo, IServiceInfo
    {
        readonly string _serviceFullName;
        readonly IServiceInfo _generalization;
        readonly IAssemblyInfo _assemblyInfo;
        readonly List<IPluginInfo> _implementations;

        internal ServiceInfo( string serviceFullName, IAssemblyInfo assemblyInfo, IServiceInfo generalization = null )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceFullName ) );
            Debug.Assert( assemblyInfo != null );

            _serviceFullName = serviceFullName;
            _assemblyInfo = assemblyInfo;
            _implementations = new List<IPluginInfo>();
        }

        internal void BindPlugin( IPluginInfo plugin )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( plugin.Service == this );
            Debug.Assert( !_implementations.Contains( plugin ) );

            _implementations.Add( plugin );
        }

        #region IServiceInfo Members

        public string ServiceFullName
        {
            get { return _serviceFullName; }
        }

        public IServiceInfo Generalization
        {
            get { return _generalization; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public IReadOnlyList<IPluginInfo> Implementations
        {
            get { return _implementations.AsReadOnlyList(); }
        }

        #endregion
    }
}
