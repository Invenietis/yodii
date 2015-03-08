#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\FinalConfigurationChange.cs) is part of CiviKey. 
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
        /// <summary>
        /// No change.
        /// </summary>
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
        Cleared = 64,

        /// <summary>
        /// The whole configuration has been set.
        /// </summary>
        Set = 128,

        /// <summary>
        /// The discovered info has been changed.
        /// </summary>
        NewDiscoveredInfo = 256
    }
}
