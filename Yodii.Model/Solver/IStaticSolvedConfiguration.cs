using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{

    public interface IStaticSolvedConfiguration
    {
        IReadOnlyList<IStaticSolvedPlugin> Plugins { get; }
        IReadOnlyList<IStaticSolvedService> Service { get; }
        IStaticSolvedService FindService( string fullName );
        IStaticSolvedPlugin FindPlugin( Guid pluginId );
    }
}
