using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface ILiveServiceInfo : INotifyPropertyChanged
    {
        IServiceInfo ServiceInfo { get; }

        RunningRequirement ConfigRequirement { get; }

        RunningStatus Status { get; }

        bool IsRunning { get; }

        ILiveServiceInfo Generalization { get; }

        ILivePluginInfo RunningPlugin { get; }

        bool Start();

        void Stop();
    }
}
