#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\ConfigurationChangedEventArgs.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Details concerning a change in the Configurationmanager.
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        readonly FinalConfiguration _finalConfiguration;
        readonly FinalConfigurationChange _finalConfigurationChanged;
        readonly IConfigurationItem _configurationItemChanged;
        readonly IConfigurationLayer _configurationLayerChanged;
        readonly IDiscoveredInfo _newInfo;

        /// <summary>
        /// Gets the new FinalConfiguration.
        /// Null if the current <see cref="IConfigurationManager.DiscoveredInfo"/> has not changed.
        /// </summary>
        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        /// <summary>
        /// Gets the new discovered information if it has changed.
        /// </summary>
        public IDiscoveredInfo NewDiscoveredInfo
        {
            get { return _newInfo; }
        }

        /// <summary>
        /// Details of changes in the FinalConfiguration.
        /// </summary>
        public FinalConfigurationChange FinalConfigurationChanged
        {
            get { return _finalConfigurationChanged; }
        }

        /// <summary>
        /// The ConfigurationItem that changed.
        /// </summary>
        public IConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        /// <summary>
        /// The ConfigurationLayer that changed.
        /// </summary>
        public IConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangedEventArgs triggered by a ConfigurationItem.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Changes of the new FinalConfiguration</param>
        /// <param name="configurationItem">Item that changed</param>
        public ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangedEventArgs triggered by a ConfigurationLayer or a global change.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Changes of the new FinalConfiguration</param>
        /// <param name="configurationLayer">Layer that changed. Can be null for global change.</param>
        /// <param name="newInfo">Optional new discovered info set.</param>
        public ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationLayer configurationLayer, IDiscoveredInfo newInfo )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
            _newInfo = newInfo;
        }
    }
}
