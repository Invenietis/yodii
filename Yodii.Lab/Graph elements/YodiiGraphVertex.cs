using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model.CoreModel;

namespace Yodii.Lab
{
    public class YodiiGraphVertex
    {
        readonly bool _isPlugin;
        readonly IServiceInfo _service;
        readonly IPluginInfo _plugin;

        internal YodiiGraphVertex(IPluginInfo plugin)
        {
            _isPlugin = true;
            _plugin = plugin;
        }

        internal YodiiGraphVertex( IServiceInfo service )
        {
            _isPlugin = false;
            _service = service;
        }

        public bool IsPlugin { get { return _isPlugin; } }
        public bool IsService { get { return !_isPlugin; } }
        public IServiceInfo ServiceInfo { get { return _service; } }
        public IPluginInfo PluginInfo { get { return _plugin; } }
    }
}
