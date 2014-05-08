using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IDiscoverer
    {
        IDiscoveredInfo GetDiscoveredInfo( bool withAssembliesOnError = false );
    }
}
