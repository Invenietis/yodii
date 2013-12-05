using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    /// <summary>
    /// Collection of configuration layers.
    /// <seealso cref="IConfigurationManager"/>
    /// </summary>
    public interface IConfigurationLayerCollection : ICKObservableReadOnlyList<IConfigurationLayer>
    {
        /// <summary>
        /// Creates an empty layer in the collection.
        /// </summary>
        /// <param name="layerName">Display name of the new layer</param>
        /// <returns>Created layer</returns>
        IConfigurationLayer Create( string layerName = null );

        /// <summary>
        /// Attempts to remove a configuration layer from the collection.
        /// </summary>
        /// <param name="layer">IConfigurationLayer to remove</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Remove( IConfigurationLayer layer );

        /// <summary>
        /// Accesses an item of this collection.
        /// </summary>
        /// <param name="layerName">Layer display name</param>
        /// <returns>Layer item</returns>
        IReadOnlyCollection<IConfigurationLayer> this[string layerName] { get; }
    }
}
