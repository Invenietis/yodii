using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;


namespace Yodii.Engine.Tests.Mocks
{
    public class ServiceReferenceInfo : IServiceReferenceInfo
    {
        IPluginInfo _owner;
        IServiceInfo _reference;
        DependencyRequirement _requirement;
        string _ctorParamOrPropertyName;
        int _ctorParamIndex;
        bool _isServiceWrapped;

        internal ServiceReferenceInfo( IPluginInfo ownerPlugin, IServiceInfo referencedService, DependencyRequirement requirement )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
        }

        public IPluginInfo Owner
        {
            get { return _owner; }
            set { _owner = value; }
        }

        public IServiceInfo Reference
        {
            get { return _reference; }
            set { _reference = value; }
        }

        public DependencyRequirement Requirement
        {
            get { return _requirement; }
            set { _requirement = value; }
        }

        public string ConstructorParameterOrPropertyName
        {
            get { return _ctorParamOrPropertyName; }
            set { _ctorParamOrPropertyName = value; }
        }


        public int ConstructorParameterIndex
        {
            get { return _ctorParamIndex; }
            set { _ctorParamIndex = value; }
        }

        public bool IsIServiceWrapped
        {
            get { return _isServiceWrapped; }
            set { _isServiceWrapped = value; }
        }
    }
}
