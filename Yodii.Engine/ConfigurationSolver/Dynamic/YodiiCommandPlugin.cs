using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Yodii.Model;

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

        internal void Execute()
        {
            var target = _availablePlugins.FirstOrDefault( p => p.Value.PluginInfo.PluginId == _pluginId );
            if ( _start )
            {
                Debug.Assert( target.Key.Disabled != true && target.Key.Status == RunningStatus.Stopped );
                target.Key.DynamicStart();
            }
            else
            {
                Debug.Assert( target.Key.Disabled != true && target.Key.Status == RunningStatus.Running );
                target.Key.DynamicStop();
            }
        }      
    }
}
