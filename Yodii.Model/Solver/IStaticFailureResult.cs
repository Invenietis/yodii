using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{

    public interface IStaticFailureResult
    {
        IStaticSolvedConfiguration SolvedConfiguration { get; }

        IReadOnlyList<IStaticSolvedPlugin> BlockingPlugins { get; }

        IReadOnlyList<IStaticSolvedService> BlockingServices { get; }
    }
}
