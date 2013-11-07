using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Engine
{
    internal class YodiiCommand
    {
        Guid _pluginId;
        string _serviceFullName;
        bool _start;

        internal YodiiCommand( Guid id, bool start )
        {
            _pluginId = id;
            _start = start;
        }
        internal YodiiCommand( string serviceFullName, bool start )
        {
            _serviceFullName = serviceFullName;
            _start = start;
        }

        internal void StartPlugin( Guid id, bool start )
        {            
        }
        internal void StopPlugin( Guid id, bool start )
        {
        }
        internal void StartService( string serviceFullName, bool start )
        {
        }
        internal void StopService( string serviceFullName, bool start )
        {
        }
    }
}
