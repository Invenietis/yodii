using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;
using System.Collections;

namespace Yodii.Lab.Mocks
{
    public class ServiceInfo : IServiceInfo
    {
        readonly string _serviceFullName;
        readonly IServiceInfo _generalization;
        readonly IAssemblyInfo _assemblyInfo;
        readonly CKObservableSortedArrayList<PluginInfo> _implementations;

        internal ServiceInfo( string serviceFullName, IAssemblyInfo assemblyInfo, IServiceInfo generalization = null )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceFullName ) );
            Debug.Assert( assemblyInfo != null );

            _serviceFullName = serviceFullName;
            _assemblyInfo = assemblyInfo;
            _generalization = generalization;
            _implementations = new CKObservableSortedArrayList<PluginInfo>((a, b) => CaseInsensitiveComparer.Default.Compare(a.PluginFullName, b.PluginFullName), false);
        }

        internal CKObservableSortedArrayList<PluginInfo> InternalImplementations
        {
            get
            {
                return _implementations;
            }
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

        public bool HasError
        {
            // TODO
            get { throw new NotImplementedException(); }
        }

        public string ErrorMessage
        {
            // TODO
            get { throw new NotImplementedException(); }
        }
        public override string ToString()
        {
            return String.Format( "{0} ", _serviceFullName);
        }
    }
}
