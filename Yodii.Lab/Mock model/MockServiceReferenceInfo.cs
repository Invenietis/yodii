using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;


namespace Yodii.Lab.Mocks
{
    /// <summary>
    /// Mock IServiceReferenceInfo.
    /// </summary>
    [DebuggerDisplay( "=> {Reference.ServiceFullName} ({Requirement})" )]
    public class MockServiceReferenceInfo : ViewModelBase, IServiceReferenceInfo
    {
        readonly PluginInfo _owner;
        readonly ServiceInfo _reference;

        DependencyRequirement _requirement;

        internal MockServiceReferenceInfo( PluginInfo ownerPlugin, ServiceInfo referencedService, DependencyRequirement requirement )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
        }
        #region IServiceReferenceInfo Members

        IPluginInfo IServiceReferenceInfo.Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Plugin owner.
        /// </summary>
        public PluginInfo Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Service ref.
        /// </summary>
        public IServiceInfo Reference
        {
            get { return _reference; }
        }

        DependencyRequirement IServiceReferenceInfo.Requirement
        {
            get { return _requirement; }
        }

        /// <summary>
        /// Requirement.
        /// </summary>
        public DependencyRequirement Requirement
        {
            get { return _requirement; }
            set
            {
                if( value != _requirement)
                {
                    _requirement = value;
                    RaisePropertyChanged( "Requirement" );
                }
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public string ConstructorParameterOrPropertyName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public int ConstructorParameterIndex
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public bool IsIServiceWrapped
        {
            get { throw new NotImplementedException(); }
        }

        #endregion


        /// <summary>
        /// Not implemented.
        /// </summary>
        public string ConstructorParmeterOrPropertyName
        {
            get { throw new NotImplementedException(); }
        }
    }
}
