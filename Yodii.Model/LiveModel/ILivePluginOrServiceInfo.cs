using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Live plugin or service info.
    /// </summary>
    public interface ILivePluginOrServiceInfo : INotifyPropertyChanged
    {
        /// <summary>
        /// Whether the plugin is running.
        /// </summary>
        bool IsRunning { get; }
    }
}
