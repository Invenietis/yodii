using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;
using Yodii.Model.CoreModel;

namespace Yodii.Lab.Mocks
{
    public class PluginInfo : IPluginInfo
    {
        readonly Guid _guid;
        readonly string _pluginFullName;
        readonly IAssemblyInfo _assemblyInfo;
        readonly CKObservableSortedArrayList<MockServiceReferenceInfo> _serviceReferences;
        readonly ServiceInfo _service;

        internal PluginInfo( Guid guid, string pluginFullName, IAssemblyInfo assemblyInfo, ServiceInfo service = null )
        {
            Debug.Assert( guid != null );
            Debug.Assert( !String.IsNullOrEmpty( pluginFullName ) );
            Debug.Assert( assemblyInfo != null );

            _guid = guid;
            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;

            _service = service;
            _serviceReferences = new CKObservableSortedArrayList<MockServiceReferenceInfo>();
        }

        internal CKObservableSortedArrayList<MockServiceReferenceInfo> InternalServiceReferences
        {
            get { return _serviceReferences; }
        }

        internal ServiceInfo InternalService
        {
            get { return _service; }
        }

        #region IPluginInfo Members

        public Guid PluginId
        {
            get { return _guid; }
        }

        public string PluginFullName
        {
            get { return _pluginFullName; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public IReadOnlyList<IServiceReferenceInfo> ServiceReferences
        {
            get { return _serviceReferences.AsReadOnlyList(); }
        }

        public IServiceInfo Service
        {
            get { return _service; }
        }

        #endregion

        public bool HasError
        {
            get { throw new NotImplementedException(); }
        }

        public string ErrorMessage
        {
            get { throw new NotImplementedException(); }
        }
    }
}
