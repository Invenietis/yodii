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

        internal void StartPlugin( Guid id, bool start )
        {
            var target = _availablePlugins.FirstOrDefault( p => p.Value.PluginInfo.PluginId == id );
            Debug.Assert( target.Key.Disabled != true && target.Key.Status == RunningStatus.Stopped);
            //Call PluginData.Dynamic method which dynamically starts the plugin
            //if(target.Key != null) 
        }
        internal void StopPlugin( Guid id, bool start )
        {
            var target = _availablePlugins.FirstOrDefault( p => p.Value.PluginInfo.PluginId == id );
            Debug.Assert( target.Key.Disabled != true && target.Key.Status == RunningStatus.Running );
            //Call PluginData.Dynamic method which dynamically stops the plugin
        }       
    }
}
