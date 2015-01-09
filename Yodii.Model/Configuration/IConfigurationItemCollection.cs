#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\IConfigurationItemCollection.cs) is part of CiviKey. 
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
namespace Yodii.Model
{
    /// <summary>
    /// Collection of configuration items hold by a <see cref="IConfigurationLayer"/>.
    /// </summary>
    public interface IConfigurationItemCollection : ICKObservableReadOnlyList<IConfigurationItem>
    {
        /// <summary>
        /// Attempts to add or set a configuration item with a status and an impact.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name.</param>
        /// <param name="status">Required configuration status.</param>
        /// <param name="impact">Required impact.</param>
        /// <param name="description">Optional description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Set( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact, string description = null );

        /// <summary>
        /// Attempts to add or set a status on an item.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name.</param>
        /// <param name="status">Required configuration status.</param>
        /// <param name="description">Optional description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Set( string serviceOrPluginFullName, ConfigurationStatus status, string description = null );

        /// <summary>
        /// Attempts to add or set an impact on an item.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name.</param>
        /// <param name="impact">Required impact.</param>
        /// <param name="description">Optional description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Set( string serviceOrPluginFullName, StartDependencyImpact impact, string description = null );

        /// <summary>
        /// Attempts to remove a configuration item, effectively making it Optional.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name to remove.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Remove( string serviceOrPluginFullName );

        /// <summary>
        /// Gets the <see cref="IConfigurationItem"/> for the given plugin or service.
        /// </summary>
        /// <param name="key">Service or Plugin full name</param>
        /// <returns>Configuration item.</returns>
        IConfigurationItem this[string key] { get; }

        /// <summary>
        /// Gets the <see cref="IConfigurationLayer"/> that contains this collection.
        /// </summary>
        IConfigurationLayer ParentLayer { get; }
    }
}
