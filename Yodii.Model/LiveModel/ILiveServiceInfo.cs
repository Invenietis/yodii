using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Live status of a service, when the engine is started.
    /// </summary>
    public interface ILiveServiceInfo : ILiveYodiiItem, IDynamicSolvedService, INotifyPropertyChanged
    {
        /// <summary>
        /// Live generalization, if this service has one.
        /// </summary>
        ILiveServiceInfo Generalization { get; }

        /// <summary>
        /// Running plugin that implements this service when started.
        /// </summary>
        ILivePluginInfo RunningPlugin { get; }

        /// <summary>
        /// Last running plugin.
        /// </summary>
        ILivePluginInfo LastRunningPlugin { get; }

    }
}
