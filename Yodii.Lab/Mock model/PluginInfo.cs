﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    /// <summary>
    /// Mock plugin info, for Yodii.Lab.
    /// </summary>
    [DebuggerDisplay( "{PluginFullName} = {PluginId}" )]
    public class PluginInfo : ViewModelBase, IPluginInfo
    {
        readonly Guid _guid;
        readonly IAssemblyInfo _assemblyInfo;
        readonly CKObservableSortedArrayList<MockServiceReferenceInfo> _serviceReferences;

        bool _hasError;
        string _errorMessage;

        ServiceInfo _service;
        string _pluginFullName;

        internal PluginInfo( Guid guid, string pluginFullName, IAssemblyInfo assemblyInfo, ServiceInfo service = null )
        {
            Debug.Assert( guid != null );
            Debug.Assert( assemblyInfo != null );

            _guid = guid;
            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;

            _service = service;
            _serviceReferences = new CKObservableSortedArrayList<MockServiceReferenceInfo>( ( x, y ) => String.CompareOrdinal( x.Reference.ServiceFullName, y.Reference.ServiceFullName ), false );
        }

        internal CKObservableSortedArrayList<MockServiceReferenceInfo> InternalServiceReferences
        {
            get { return _serviceReferences; }
        }

        internal ServiceInfo InternalService
        {
            get { return _service; }
        }

        /// <summary>
        /// Description of this PluginInfo.
        /// </summary>
        public string Description
        {
            get
            {
                if( String.IsNullOrWhiteSpace( PluginFullName ) )
                {
                    return String.Format( "Unnamed plugin ({0})", PluginId.ToString() );
                }
                else
                {
                    return String.Format( "{0}", PluginFullName );
                }
            }
        }

        #region IPluginInfo Members

        /// <summary>
        /// Plugin GUID
        /// </summary>
        public Guid PluginId
        {
            get { return _guid; }
        }

        /// <summary>
        /// Plugin display name
        /// </summary>
        public string PluginFullName
        {
            get { return _pluginFullName; }
            set
            {
                if( value != _pluginFullName )
                {
                    _pluginFullName = value;
                    RaisePropertyChanged( "PluginFullName" );
                }
            }
        }

        /// <summary>
        /// Plugin assembly
        /// </summary>
        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        /// <summary>
        /// Plugin's service references
        /// </summary>
        public IReadOnlyList<IServiceReferenceInfo> ServiceReferences
        {
            get { return _serviceReferences.AsReadOnlyList(); }
        }

        IServiceInfo IPluginInfo.Service
        {
            get { return _service; }
        }
        /// <summary>
        /// Mock ServiceInfo of Service
        /// </summary>
        public ServiceInfo Service
        {
            get { return _service; }
            set
            {
                if( value != _service )
                {
                    if( _service != null )
                        _service.InternalImplementations.Remove( this );

                    _service = value;

                    if( _service != null )
                        _service.InternalImplementations.Add( this );

                    RaisePropertyChanged( "Service" );
                }
            }
        }

        #endregion

        /// <summary>
        /// True if plugin has error
        /// </summary>
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

        /// <summary>
        /// Plugin error message
        /// </summary>
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

        /// <summary>
        /// Returns plugin information.
        /// </summary>
        /// <returns>Plugin information.</returns>
        public override string ToString()
        {
            return String.Format( "{0} has service {1}", _pluginFullName, _service );
        }
    }
}
