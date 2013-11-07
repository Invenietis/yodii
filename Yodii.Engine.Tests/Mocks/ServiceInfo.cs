using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine.Tests.Mocks
{
    public class ServiceInfo : IServiceInfo
    {
        readonly string _serviceFullName;
        readonly IAssemblyInfo _assemblyInfo;
        readonly List<IPluginInfo> _implementations;
        IServiceInfo _generalization;
        bool _hasError; 

        internal ServiceInfo( string serviceFullName, IAssemblyInfo assemblyInfo )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceFullName ) );
            Debug.Assert( assemblyInfo != null );

            _serviceFullName = serviceFullName;
            _assemblyInfo = assemblyInfo;
            _implementations = new List<IPluginInfo>();
        }

        internal void AddPlugin( IPluginInfo plugin )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( plugin.Service == this );
            Debug.Assert( !_implementations.Contains( plugin ) );
            _implementations.Add( plugin );
        }

        internal void RemovePlugin( IPluginInfo plugin )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( plugin.Service == this );
            Debug.Assert( _implementations.Contains( plugin ) );
            _implementations.Remove( plugin );
        }

        #region IServiceInfo Members

        public string ServiceFullName
        {
            get { return _serviceFullName; }
        }

        public IServiceInfo Generalization
        {
            get { return _generalization; }
            set { _generalization = value; }
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

        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; }
        }

        public string ErrorMessage
        {
            get { return _hasError ? "An error occured." : null; }
        }

    }
}
