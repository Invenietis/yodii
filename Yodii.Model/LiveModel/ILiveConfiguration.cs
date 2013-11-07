using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;
using Yodii.Model.LiveModel;

namespace Yodii.Model
{
    interface ILiveConfiguration
    {
        IReadOnlyList<ILivePluginInfo> PluginLiveInfo { get; }

        IReadOnlyList<ILiveServiceInfo> ServiceLiveInfo { get; }

        IReadOnlyList<YodiiCommand> YodiiCommands { get; }
    }
}
