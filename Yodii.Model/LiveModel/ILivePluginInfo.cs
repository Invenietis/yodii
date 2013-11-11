using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface ILivePluginInfo : INotifyPropertyChanged
    {
        IPluginInfo PluginInfo { get; }

        RunningRequirement ConfigRequirement { get; }

        RunningStatus Status { get; }

        bool IsRunning { get; }

        ILiveServiceInfo Service { get; }

        bool Start();

        void Stop();
    }
}
