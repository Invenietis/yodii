using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Solved dynamic plugin data.
    /// </summary>
    public interface IDynamicSolvedPlugin : IDynamicSolvedYodiiItem
    {
        /// <summary>
        /// Gets the plugin information.
        /// </summary>
        IPluginInfo PluginInfo { get; }

    }
}
