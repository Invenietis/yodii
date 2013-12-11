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
        readonly IReadOnlyList<IStaticSolvedPlugin> _blockingPlugins;
        readonly IReadOnlyList<IStaticSolvedService> _blockingServices;
        readonly IStaticSolvedConfiguration _solvedConfiguration;

        internal StaticFailureResult( IStaticSolvedConfiguration solvedConfiguration, IReadOnlyList<IStaticSolvedPlugin> blockedPlugins, IReadOnlyList<IStaticSolvedService> blockedServices )
        {
            Debug.Assert( solvedConfiguration != null && blockedPlugins != null && blockedServices != null );
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
            get { return _blockingPlugins; }
        }

        public IReadOnlyList<IStaticSolvedService> BlockingServices
        {
            get { return _blockingServices; }
        }
    }
}
