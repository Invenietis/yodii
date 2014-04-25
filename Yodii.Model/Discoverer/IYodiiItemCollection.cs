using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public interface IYodiiItemCollection 
    {
        //IDiscoveredItem this[string name] { get; }

        bool Add( IDiscoveredItem item );
    }
}
