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
    [Flags]
    public enum FinalConfigurationChange
    {
        None = 0,

        /// <summary>
        /// The status of an item changed.
        /// </summary>
        StatusChanged = 1,

        /// <summary>
        /// The impact of an item changed.
        /// </summary>
        ImpactChanged = 2,

        /// <summary>
        /// The impact of an item changed.
        /// </summary>
        StatusAndImpactChanged = StatusChanged | ImpactChanged,

        /// <summary>
        /// An item was added in a layer.
        /// </summary>
        ItemAdded = 4,

        /// <summary>
        /// An item was removed from the layer.
        /// </summary>
        ItemRemoved = 8,

        /// <summary>
        /// A layer was added.
        /// </summary>
        LayerAdded = 16,

        /// <summary>
        /// A layer was removed.
        /// </summary>
        LayerRemoved = 32,

        /// <summary>
        /// The whole configuration has been cleared.
        /// </summary>
        Cleared = 64
    }
}
