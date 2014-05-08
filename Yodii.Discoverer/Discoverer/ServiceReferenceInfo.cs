using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Discoverer
{
    internal sealed class ServiceReferenceInfo : IServiceReferenceInfo
    {
        readonly IPluginInfo _owner;
        readonly IServiceInfo _reference;
        readonly DependencyRequirement _requirement;
        readonly string _ctorParamName;
        readonly int _ctorParamIndex;
        readonly bool _isNakedRunningService;

        internal ServiceReferenceInfo( IPluginInfo ownerPlugin, IServiceInfo referencedService, DependencyRequirement requirement, string paramName, int paramIndex, bool isNakedService )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
            _ctorParamName = paramName;
            _ctorParamIndex = paramIndex;
            _isNakedRunningService = isNakedService;
        }

        public IPluginInfo Owner
        {
            get { return _owner; }
        }

        public IServiceInfo Reference
        {
            get { return _reference; }
        }

        public DependencyRequirement Requirement
        {
            get { return _requirement; }
        }

        public string ConstructorParameterName
        {
            get { return _ctorParamName; }
        }

        public int ConstructorParameterIndex
        {
            get { return _ctorParamIndex; }
        }

        public bool IsNakedRunningService
        {
            get { return _isNakedRunningService; }
        }
    }
}
