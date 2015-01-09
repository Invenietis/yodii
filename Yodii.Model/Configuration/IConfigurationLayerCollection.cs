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
    /// </summary>
    public interface IConfigurationLayerCollection : ICKObservableReadOnlyList<IConfigurationLayer>
    {
        /// <summary>
        /// Creates an empty layer in the collection.
        /// </summary>
        /// <param name="layerName">Display name of the new layer. Can not be null nor empty.</param>
        /// <returns>Created layer</returns>
        IConfigurationLayer Create( string layerName );

        /// <summary>
        /// Default configuration layer has an empty <see cref="IConfigurationLayer.LayerName"/>.
        /// </summary>
        IConfigurationLayer Default { get; }

        /// <summary>
        /// Attempts to remove a configuration layer from the collection.
        /// </summary>
        /// <param name="layer">IConfigurationLayer to remove</param>
        /// <returns>Yodii engine change result.</returns>
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
