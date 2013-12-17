using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine.Tests.Mocks
{
    public class PluginInfo : IPluginInfo
    {
        readonly string _pluginFullName;
        readonly IAssemblyInfo _assemblyInfo;
        readonly List<IServiceReferenceInfo> _serviceReferences;
        IServiceInfo _service;
        bool _hasError; 

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

        public List<IServiceReferenceInfo> ServiceReferences
        {
            get { return _serviceReferences; }
        }

        public ServiceReferenceInfo AddServiceReference( ServiceInfo service, DependencyRequirement req )
        {
            var r = new ServiceReferenceInfo( this, service, req );
            _serviceReferences.Add( r );
            return r;
        }

        public IServiceInfo Service
        {
            get { return _service; }
            set
            {
                if ( _service != null ) ((ServiceInfo)_service).RemovePlugin( this );
                _service = value;
                if ( _service != null ) ((ServiceInfo)_service).AddPlugin( this );
            }
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


        IReadOnlyList<IServiceReferenceInfo> IPluginInfo.ServiceReferences
        {
            get { return _serviceReferences.AsReadOnlyList(); }
        }
    }
}
