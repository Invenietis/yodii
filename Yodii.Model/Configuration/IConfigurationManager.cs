#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\IConfigurationManager.cs) is part of CiviKey. 
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
    /// Configuration manager interface. Contains the <see cref="DiscoveredInfo"/> and a collection of <see cref="IConfigurationLayer"/>,
    /// each having a collection of <see cref="IConfigurationItem"/>.
    /// Adding and removing layers triggers configuration resolution, and project the whole configuration into a single <see cref="FinalConfiguration"/>,
    /// which is essentially a single, read-only <see cref="IConfigurationLayer"/>.
    /// </summary>
    public interface IConfigurationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Gets the current information about available plugins and services.
        /// </summary>
        IDiscoveredInfo DiscoveredInfo { get; }

        /// <summary>
        /// Sets the discovery information that describes available plugins and services.
        /// If <see cref="IYodiiEngineExternal.IsRunning"/> is true, this can be rejected: the result will indicate the reason of the failure.
        /// </summary>
        /// <param name="dicoveredInfo">New discovered information to work with. Can not be null.</param>
        /// <returns>Engine operation result.</returns>
        IYodiiEngineResult SetDiscoveredInfo( IDiscoveredInfo dicoveredInfo );

        /// <summary>
        /// Gets a <see cref="YodiiConfiguration"/> object that is a copy of the current configuration.
        /// </summary>
        /// <returns>A new <see cref="YodiiConfiguration"/> object.</returns>
        YodiiConfiguration GetConfiguration();

        /// <summary>
        /// Sets a <see cref="YodiiConfiguration"/> object. Layers with the same name are merged by default (note that layers with a null, empty 
        /// or white space name are always into the default layer).
        /// This totally reconfigures the engine if it is possible.
        /// </summary>
        /// <param name="configuration">New configuration to apply. Can not be null.</param>
        /// <param name="mergeLayersWithSameName">False to keep multiple layers with the same name. By default layers with the same name are merged into one layer.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult SetConfiguration( YodiiConfiguration configuration, bool mergeLayersWithSameName = true );

        /// <summary>
        /// Triggered when a configuration change was not canceled, once a new FinalConfiguration is available.
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Triggered during a configuration change, and permits cancellation. Contains a temporary FinalConfiguration.
        /// </summary>
        event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;

        /// <summary>
        /// Read-only collection container of read-only configuration items.
        /// </summary>
        /// <remarks>
        /// This final configuration is automatically maintained.
        /// Any change to the configuration can be canceled thanks to <see cref="IConfigurationManager.ConfigurationChanging"/>.
        /// </remarks>
        FinalConfiguration FinalConfiguration { get; }

        /// <summary>
        /// Layers contained in this manager.
        /// </summary>
        IConfigurationLayerCollection Layers { get; }


    }
}
