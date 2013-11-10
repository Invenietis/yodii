using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface ILiveConfiguration
    {
        IReadOnlyList<ILivePluginInfo> PluginLiveInfo { get; }

        IReadOnlyList<ILiveServiceInfo> ServiceLiveInfo { get; }
    }
}
