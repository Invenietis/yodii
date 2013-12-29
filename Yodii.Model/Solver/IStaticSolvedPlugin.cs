using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Describes plugin information resulting from static resolution.
    /// </summary>
    public interface IStaticSolvedPlugin : IStaticSolvedYodiiItem
    {
        /// <summary>
        /// Static plugin information.
        /// </summary>
        IPluginInfo PluginInfo { get; }
    }
}
