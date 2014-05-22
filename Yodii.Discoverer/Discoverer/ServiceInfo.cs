using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Discoverer
{
    internal sealed class ServiceInfo : IServiceInfo, IDiscoveredItem
    {
        readonly string _serviceFullName;
        readonly IAssemblyInfo _assemblyInfo;
        IServiceInfo _generalization;
        bool _hasError;

        internal ServiceInfo( string serviceFullName, IAssemblyInfo assemblyInfo )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceFullName ) );
            Debug.Assert( assemblyInfo != null );

            _serviceFullName = serviceFullName;
            _assemblyInfo = assemblyInfo;
        }

        #region IServiceInfo Members

        public string ServiceFullName
        {
            get { return _serviceFullName; }
        }

        public IServiceInfo Generalization
        {
            get { return _generalization; }
            internal set { _generalization = value; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
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
