using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;
using System.Runtime.InteropServices;

namespace Yodii.Discoverer
{
    internal sealed class PluginInfo : IPluginInfo, IDiscoveredItem
    {
        readonly string _pluginFullName;
        readonly IAssemblyInfo _assemblyInfo;
        List<IServiceReferenceInfo> _serviceReferences;
        IServiceInfo _service;
        string _errorMessage;

        internal PluginInfo( string pluginFullName, IAssemblyInfo assemblyInfo )
        {
            Debug.Assert( !String.IsNullOrEmpty( pluginFullName ) );
            Debug.Assert( assemblyInfo != null );

            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;
            _serviceReferences = new List<IServiceReferenceInfo>();
        }

        internal void BindServiceRequirement( IServiceReferenceInfo reference )
        {
            Debug.Assert( !_serviceReferences.Contains( reference ) );
            Debug.Assert( reference.Owner == this );

            _serviceReferences.Add( reference );
        }

        #region IPluginInfo Members

        public string PluginFullName
        {
            get { return _pluginFullName; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public List<IServiceReferenceInfo> ServiceReferencess
        {
            get { return _serviceReferences; }
        }

        public IServiceInfo Service
        {
            get { return _service; }
            internal set { _service = value; }
        }

        #endregion

        public bool HasError
        {
            get { return _errorMessage != null; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        IReadOnlyList<IServiceReferenceInfo> IPluginInfo.ServiceReferences
        {
            get { return _serviceReferences.AsReadOnlyList(); }
        }
    }
}
