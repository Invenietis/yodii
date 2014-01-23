using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class DynamicFailureResult : IDynamicFailureResult
    {
        readonly IDynamicSolvedConfiguration _dynamicSolvedConfiguration;
        readonly IReadOnlyList<PluginRuntimeError> _errorPlugins;

        internal DynamicFailureResult( IDynamicSolvedConfiguration dynamicSolvedConfiguration, IReadOnlyList<PluginRuntimeError> errorPlugins )
        {
            _dynamicSolvedConfiguration = dynamicSolvedConfiguration;
            _errorPlugins = errorPlugins;
        }
        public IDynamicSolvedConfiguration SolvedConfiguration
        {
            get { return _dynamicSolvedConfiguration; }
        }

        public IReadOnlyList<PluginRuntimeError> ErrorPlugins
        {
            get { return _errorPlugins; }
        }
    }
}
