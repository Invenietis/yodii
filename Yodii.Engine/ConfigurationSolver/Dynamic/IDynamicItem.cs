using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    interface IDynamicItem
    {
        StartDependencyImpact ConfigSolvedImpact { get; }

        bool DynamicStartByCommand( StartDependencyImpact impact, bool isFirst = false );

        bool DynamicStopByCommand();
    }
}
