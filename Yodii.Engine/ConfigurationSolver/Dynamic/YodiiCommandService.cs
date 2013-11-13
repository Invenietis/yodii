using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

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

        internal void Execute()
        {
            if ( _start )
            {
                var target = _services.FirstOrDefault( s => s.Value.ServiceInfo.ServiceFullName == _serviceFullName );
                Debug.Assert( target.Key.Disabled != true && target.Key.Status == RunningStatus.Stopped );
                target.Key.DynamicStart();
            }
            else
            {
                var target = _services.FirstOrDefault( s => s.Value.ServiceInfo.ServiceFullName == _serviceFullName );
                Debug.Assert( target.Key.Disabled != true && target.Key.Status == RunningStatus.Running );
                target.Key.DynamicStop();
            }
        }
    }
}
