using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Dynamic plugin or service.
    /// </summary>
    public interface IDynamicYodiiItem
    {
        /// <summary>
        /// Running status.
        /// </summary>
        RunningStatus RunningStatus { get; }
    }
}
