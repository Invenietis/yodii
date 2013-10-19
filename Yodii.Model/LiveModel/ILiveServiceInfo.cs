using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model.CoreModel;

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
