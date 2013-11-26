using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface ILiveServiceInfo : IDynamicSolvedService, INotifyPropertyChanged
    {
        bool IsRunning { get; }

        ILiveServiceInfo Generalization { get; }

        ILivePluginInfo RunningPlugin { get; }

        ILivePluginInfo LastRunningPlugin { get; }

        bool Start( object caller );

        void Stop( object caller );
    }
}
