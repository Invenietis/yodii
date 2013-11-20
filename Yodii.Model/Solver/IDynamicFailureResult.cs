using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IDynamicFailureResult
    {
        IDynamicSolvedConfiguration SolvedConfiguration { get; }

        IReadOnlyList<PluginRuntimeError> ErrorPlugins { get; }
    }
}
