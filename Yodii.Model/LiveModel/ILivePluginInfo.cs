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
    /// Live status of a plugin, when the engine is started.
    /// </summary>
    public interface ILivePluginInfo : ILiveYodiiItem, IDynamicSolvedPlugin, INotifyPropertyChanged
    {
        /// <summary>
        /// The live service this plugin implements.
        /// </summary>
        /// <remarks>Can be null.</remarks>
        ILiveServiceInfo Service { get; }

        /// <summary>
        /// Runtime plugin exception.
        /// </summary>
        IPluginHostApplyCancellationInfo CurrentError { get; }
    }
}
