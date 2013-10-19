using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;
using Yodii.Model.CoreModel;


namespace Yodii.Engine.Tests.Mocks
{
    public class MockServiceReferenceInfo : IServiceReferenceInfo
    {
        readonly IPluginInfo _owner;
        readonly IServiceInfo _reference;
        readonly RunningRequirement _requirement;

        internal MockServiceReferenceInfo( IPluginInfo ownerPlugin, IServiceInfo referencedService, RunningRequirement requirement )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
        }
        #region IServiceReferenceInfo Members

        public IPluginInfo Owner
        {
            get { return _owner; }
        }

        public IServiceInfo Reference
        {
            get { return _reference; }
        }

        public RunningRequirement Requirement
        {
            get { return _requirement; }
        }

        public string ConstructorParameterOrPropertyName
        {
            get { throw new NotImplementedException(); }
        }

        public int ConstructorParameterIndex
        {
            get { throw new NotImplementedException(); }
        }

        public bool IsIServiceWrapped
        {
            get { throw new NotImplementedException(); }
        }

        #endregion
    }
}
