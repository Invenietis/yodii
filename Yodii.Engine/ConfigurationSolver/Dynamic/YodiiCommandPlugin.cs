using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Engine
{
    internal class YodiiCommandPlugin : YodiiCommand
    {
        readonly Guid _pluginId;
        readonly bool _start; 

        internal YodiiCommandPlugin( Guid id, bool start )
        {
            _pluginId = id;
            _start = start;
        }  

        internal void StartPlugin( Guid id, bool start )
        {
            
        }
        internal void StopPlugin( Guid id, bool start )
        {
        }       
    }
}
