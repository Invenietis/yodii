#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\IConfigurationItem.cs) is part of CiviKey. 
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
using System.ComponentModel;
namespace Yodii.Model
{
    /// <summary>
    /// Configuration item bound to a <see cref="IConfigurationManager"/>.
    /// </summary>
    public interface IConfigurationItem : IConfigurationItemData, INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the parent configuration layer.
        /// </summary>
        IConfigurationLayer Layer { get; }

        /// <summary>
        /// Attempts to change the required <see cref="IConfigurationItemData.Status">Status</see> of this item.
        /// </summary>
        /// <param name="newStatus">New status for this item.</param>
        /// <param name="newDescription">Optional description to set. When null, current description is preserved.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult Set( ConfigurationStatus newStatus, string newDescription = null );
        
        /// <summary>
        /// Attempts to change the required <see cref="IConfigurationItemData.Impact">Impact</see> of this item.
        /// </summary>
        /// <param name="newImpact">New impact for this item.</param>
        /// <param name="newDescription">Optional description to set. When null, current description is preserved.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult Set( StartDependencyImpact newImpact, string newDescription = null );

        /// <summary>
        /// Attempts to change the <see cref="IConfigurationItemData.Status">Status</see> and the <see cref="IConfigurationItemData.Impact">Impact</see> and optionaly sets the description.
        /// </summary>
        /// <param name="newStatus">New status for this item.</param>
        /// <param name="newImpact">New impact for this item.</param>
        /// <param name="newDescription">Optional description to set. When null, current description is preserved.</param>
        /// <returns>Engine change result.</returns>
        IYodiiEngineResult Set( ConfigurationStatus newStatus, StartDependencyImpact newImpact, string newDescription = null );

        /// <summary>
        /// Gets or sets an optional description for this configuration.
        /// This is null when <see cref="Layer"/> is null (this item does no more belong to its layer).
        /// </summary>
        new string Description { get; set; }
    }
}
