using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Engine
{
    internal class YodiiCommandService : YodiiCommand
    {
        readonly string _serviceFullName;
        readonly bool _start;

        internal YodiiCommandService( string serviceFullName, bool start )
        {
            _serviceFullName = serviceFullName;
            _start = start;
        }

        internal void StartService( string serviceFullName, bool start )
        {
        }
        internal void StopService( string serviceFullName, bool start )
        {
        }
    }
}
