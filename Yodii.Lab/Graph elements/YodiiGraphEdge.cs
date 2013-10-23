using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using QuickGraph;

namespace Yodii.Lab
{
    class YodiiGraphEdge : Edge<YodiiGraphVertex>
    {
        public YodiiGraphEdge( YodiiGraphVertex source, YodiiGraphVertex target )
            : base( source, target )
        {
        }
    }
}
