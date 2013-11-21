using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;

namespace Yodii.Engine
{
    public class StaticFailureResult : IStaticFailureResult
    {
        readonly List<IStaticSolvedPlugin> _blockingPlugins;
        readonly List<IStaticSolvedService> _blockingServices;
        readonly IStaticSolvedConfiguration _solvedConfiguration;

        internal StaticFailureResult( IStaticSolvedConfiguration solvedConfiguration, List<IStaticSolvedPlugin> blockedPlugins, List<IStaticSolvedService> blockedServices )
        {
            Debug.Assert( blockedPlugins != null && blockedServices != null );
            _blockingPlugins = blockedPlugins;
            _blockingServices = blockedServices;
            _solvedConfiguration = solvedConfiguration;
        }

        public IStaticSolvedConfiguration StaticSolvedConfiguration
        {
            get { return _solvedConfiguration; }
        }
        public IReadOnlyList<IStaticSolvedPlugin> BlockingPlugins
        {
            get { return _blockingPlugins.AsReadOnlyList(); }
        }
        public IReadOnlyList<IStaticSolvedService> BlockingServices
        {
            get { return _blockingServices.AsReadOnlyList(); }
        }
    }
}
