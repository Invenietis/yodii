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
    public class ServiceInfo : ViewModelBase, IServiceInfo
    {
        readonly string _serviceFullName;
        readonly IAssemblyInfo _assemblyInfo;
        readonly CKObservableSortedArrayList<PluginInfo> _implementations;

        IServiceInfo _generalization;

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
            set
            {
                if( value != _generalization )
                {
                    _generalization = value;
                    RaisePropertyChanged( "Generalization" );
                }
            }
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
    }
}
