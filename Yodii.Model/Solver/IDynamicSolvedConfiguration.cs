using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IDynamicSolvedConfiguration
    {
        IReadOnlyList<IDynamicSolvedPlugin> Plugins { get; }
        IReadOnlyList<IDynamicSolvedService> Services { get; }
        IDynamicSolvedService FindService( string fullName );
        IDynamicSolvedPlugin FindPlugin( Guid pluginId );
    }
}
