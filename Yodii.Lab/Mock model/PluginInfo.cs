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
        readonly List<IServiceReferenceInfo> _serviceReferences;
        readonly IServiceInfo _service;

        internal PluginInfo( Guid guid, string pluginFullName, IAssemblyInfo assemblyInfo, IServiceInfo service = null )
        {
            Debug.Assert( guid != null );
            Debug.Assert( !String.IsNullOrEmpty( pluginFullName ) );
            Debug.Assert( assemblyInfo != null );

            _guid = guid;
            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;

            _service = service;
            _serviceReferences = new List<IServiceReferenceInfo>();
        }

        internal void BindServiceRequirement( IServiceReferenceInfo reference )
        {
            Debug.Assert( !_serviceReferences.Contains( reference ) );
            Debug.Assert( reference.Owner == this );

            _serviceReferences.Add( reference );
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
