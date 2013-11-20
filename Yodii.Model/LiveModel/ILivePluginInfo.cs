using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public interface ILivePluginInfo : IDynamicSolvedPlugin, INotifyPropertyChanged
    {
        bool IsRunning { get; }

        ILiveServiceInfo Service { get; }

        bool Start( object caller, StartDependencyImpact impact = StartDependencyImpact.None );

        void Stop( object caller );

        Exception CurrentError { get; }
    }
}
