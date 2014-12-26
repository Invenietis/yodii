using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    internal class SolvedPluginSnapshot : SolvedItemSnapshot, IStaticSolvedPlugin, IDynamicSolvedPlugin
    {
        readonly IPluginInfo _pluginInfo;

        public SolvedPluginSnapshot( PluginData plugin )
            : base( plugin )
        {
            _pluginInfo = plugin.PluginInfo;
        }

        public bool IsPlugin { get { return true; } }

        public override string FullName { get { return _pluginInfo.PluginFullName; } }
        
        public IPluginInfo PluginInfo { get { return _pluginInfo; } }
    }
}
