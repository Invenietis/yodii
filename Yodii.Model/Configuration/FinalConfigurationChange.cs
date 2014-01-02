using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Detail of which type of change triggered the FinalConfiguration change.
    /// </summary>
    public enum FinalConfigurationChange
    {
        /// <summary>
        /// The status of an item changed.
        /// </summary>
        StatusChanged,

        /// <summary>
        /// An item was added in a layer.
        /// </summary>
        ItemAdded,

        /// <summary>
        /// An item was removed from the layer.
        /// </summary>
        ItemRemoved,

        /// <summary>
        /// A layer was added.
        /// </summary>
        LayerAdded,

        /// <summary>
        /// A layer was removed.
        /// </summary>
        LayerRemoved,

        /// <summary>
        /// The whole configuration has been cleared.
        /// </summary>
        Cleared
    }
}
