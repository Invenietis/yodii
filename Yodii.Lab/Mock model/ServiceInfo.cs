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
    [DebuggerDisplay( "{ServiceFullName}" )]
    public class ServiceInfo : ViewModelBase, IServiceInfo
    {
        readonly IAssemblyInfo _assemblyInfo;
        readonly CKObservableSortedArrayList<PluginInfo> _implementations;
        bool _hasError;
        string _errorMessage;

        IServiceInfo _generalization;
        string _serviceFullName;

        internal ServiceInfo( string serviceFullName, IAssemblyInfo assemblyInfo, IServiceInfo generalization = null )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceFullName ) );
            Debug.Assert( assemblyInfo != null );

            _serviceFullName = serviceFullName;
            _assemblyInfo = assemblyInfo;
            _generalization = generalization;
            _implementations = new CKObservableSortedArrayList<PluginInfo>( ( a, b ) => CaseInsensitiveComparer.Default.Compare( a.PluginFullName, b.PluginFullName ), false );
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
            set
            {
                if( value != _serviceFullName )
                {
                    _serviceFullName = value;
                    RaisePropertyChanged( "ServiceFullName" );
                }
            }
        }

        public IServiceInfo Generalization
        {
            get { return _generalization; }
            set
            {
                if( value != _generalization )
                {
                    // Safeguard against loops
                    if( value != this && ( value == null || !((ServiceInfo)value).SpecializesService(this) ) )
                    {
                        _generalization = value;
                    }
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
            get { return _hasError; }
            set
            {
                if( value != _hasError )
                {
                    _hasError = value;
                    RaisePropertyChanged();
                }
            }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                if( value != _errorMessage )
                {
                    _errorMessage = value;
                    RaisePropertyChanged();
                }
            }
        }

        public override string ToString()
        {
            return String.Format( "{0} ", _serviceFullName );
        }

        /// <summary>
        /// Checks if this service is already has the one specified as parameter in his Generalization tree.
        /// </summary>
        /// <param name="s">Service to check against.</param>
        /// <returns>True if service is in this instance's Generalization tree, False if it isn't.</returns>
        /// <remarks>May cause a stack overflow in case of Generalization loop. TODO</remarks>
        public bool SpecializesService( IServiceInfo service )
        {
            IServiceInfo s = Generalization;
            while( s != null )
            {
                if( s == service )
                {
                    return true;
                }
                s = s.Generalization;
            }
            return false;
        }
    }
}
