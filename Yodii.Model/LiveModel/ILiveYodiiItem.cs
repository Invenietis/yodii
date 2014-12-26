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
    public interface ILiveYodiiItem : IDynamicSolvedYodiiItem, INotifyPropertyChanged
    {
        /// <summary>
        /// Whether this live Yodii item (plugin or service), is running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets the <see cref="ILiveRunCapability"/> for this item.
        /// </summary>
        ILiveRunCapability Capability { get; }
    }
}
