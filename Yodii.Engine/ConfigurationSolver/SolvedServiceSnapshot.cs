using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class SolvedServiceSnapshot : SolvedItemSnapshot, IStaticSolvedService, IDynamicSolvedService
    {
        readonly IServiceInfo _serviceInfo;

        public SolvedServiceSnapshot( ServiceData s )
            : base( s )
        {
            _serviceInfo = s.ServiceInfo;
        }

        public override string FullName { get { return _serviceInfo.ServiceFullName; } }
        
        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }
    }
}
