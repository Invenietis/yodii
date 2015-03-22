#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\IConfigurationLayerCollection.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Core;
using System.Collections.Generic;

namespace Yodii.Model
{
    /// <summary>
    /// Collection of configuration layers contained in a <see cref="IConfigurationManager"/>.
    /// <see cref="IConfigurationLayer.LayerName">Layer's name</see> is not an identifier: except for 
    /// the default layer for which name is <see cref="String.Empty"/>
    /// and that is necessarily unique, multiple layers can have the same name.
    /// </summary>
    public interface IConfigurationLayerCollection : ICKObservableReadOnlyList<IConfigurationLayer>
    {
        /// <summary>
        /// Creates an empty layer in the collection.
        /// </summary>
        /// <param name="layerName">Display name of the new layer. Can not be null nor empty.</param>
        /// <returns>Created layer.</returns>
        IConfigurationLayer Create( string layerName );

        /// <summary>
        /// Finds an existing layer from its name or creates an empty layer in the collection.
        /// </summary>
        /// <param name="layerName">Display name of the layer. When null or empty, the <see cref="Default"/> layer is returned.</param>
        /// <returns>Found or created layer.</returns>
        IConfigurationLayer FindOneOrCreate( string layerName );

        /// <summary>
        /// Finds an existing layer from its name in the collection. When more than one layer exist with this name,
        /// one of them is returned that is not necessarily the first one.
        /// </summary>
        /// <param name="layerName">Display name of the layer. When null or empty, the <see cref="Default"/> layer is returned.</param>
        /// <returns>One of the layer with the name or null if not found.</returns>
        IConfigurationLayer FindOne( string layerName );

        /// <summary>
        /// Default configuration layer has an empty <see cref="IConfigurationLayer.LayerName"/>.
        /// </summary>
        IConfigurationLayer Default { get; }

        /// <summary>
        /// Attempts to remove a configuration layer from the collection.
        /// </summary>
        /// <param name="layer">IConfigurationLayer to remove</param>
        /// <returns>Yodii engine change result.</returns>
        /// <remarks>
        /// Since removing a layer removes constraints on the system, it seems obvious that this method can never fail.
        /// However, this method returns a <see cref="IYodiiEngineResult"/> because of <see cref="IConfigurationManager.ConfigurationChanging"/> 
        /// event that may reject the removal.
        /// </remarks>
        IYodiiEngineResult Remove( IConfigurationLayer layer );

        /// <summary>
        /// Gets layers by their name.
        /// </summary>
        /// <param name="layerName">Layer display name.</param>
        /// <returns>Zero, one or more layers.</returns>
        IReadOnlyCollection<IConfigurationLayer> this[string layerName] { get; }

        /// <summary>
        /// Clears any existing configuration (removes any existing layers).
        /// </summary>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Clear();

    }
}
