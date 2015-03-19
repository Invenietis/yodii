#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\ConfigurationChangingEventArgs.cs) is part of CiviKey. 
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
using CK.Core;
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Details of a ConfigurationChanging event.
    /// </summary>
    public class ConfigurationChangingEventArgs : EventArgs
    {
        readonly FinalConfiguration _finalConfiguration;
        readonly FinalConfigurationChange _finalConfigurationChange;
        readonly IConfigurationItem _configurationItemChanged;
        readonly IConfigurationLayer _configurationLayerChanged;
        readonly IDiscoveredInfo _newDiscoveredInfo;

        List<string> _externalReasons;

        /// <summary>
        /// Gets the new FinalConfiguration.
        /// </summary>
        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }


        /// <summary>
        /// Gets the new <see cref="IDiscoveredInfo"/> if it is changing. 
        /// Null if the current <see cref="IConfigurationManager.DiscoveredInfo"/> is not changing.
        /// </summary>
        public IDiscoveredInfo NewDiscoveredInfo
        {
            get { return _newDiscoveredInfo; }
        }
        
        /// <summary>
        /// Details of changes in the FinalConfiguration.
        /// </summary>
        public FinalConfigurationChange FinalConfigurationChange
        {
            get { return _finalConfigurationChange; }
        }

        /// <summary>
        /// Changed item.
        /// </summary>
        public IConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        /// <summary>
        /// Changed layer.
        /// </summary>
        public IConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }


        /// <summary>
        /// Whether the change was canceled.
        /// </summary>
        public bool IsCanceled
        {
            get { return _externalReasons != null; }
        }

        /// <summary>
        /// List of reasons of why the change was canceled. Never null.
        /// </summary>
        public IReadOnlyList<string> FailureExternalReasons
        {
            get { return _externalReasons != null ? _externalReasons.AsReadOnlyList() : CKReadOnlyListEmpty<string>.Empty; }
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangingEventArgs for a global change.
        /// </summary>
        /// <param name="finalConfiguration">
        /// New FinalConfiguration. 
        /// When empty <see cref="P:FinalConfigurationChange"/> is set to <see cref="T:FinalConfigurationChange.Cleared"/>, 
        /// otherwise it is set to <see cref="T:FinalConfigurationChange.Set"/>.
        /// </param>
        /// <param name="newInfo">Not null if the discovered information is a new one.</param>
        public ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, IDiscoveredInfo newInfo = null )
        {
            if( finalConfiguration == null ) throw new ArgumentException( "Must be not null and empty.", "finalConfiguration" );
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfiguration.Items.Count == 0 ? FinalConfigurationChange.Cleared : FinalConfigurationChange.Set;
            if( (_newDiscoveredInfo = newInfo) != null ) _finalConfigurationChange |= FinalConfigurationChange.NewDiscoveredInfo;
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangingEventArgs when change is triggered by a ConfigurationItem.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Details of changes in the new FinalConfiguration</param>
        /// <param name="configurationItem">Item that provoked this change</param>
        public ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangingEventArgs when change is triggered by a ConfigurationLayer.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Details of changes in the new FinalConfiguration</param>
        /// <param name="configurationLayer">Layer that provoked this change</param>
        public ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationLayer configurationLayer )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }

        /// <summary>
        /// Cancel this change with a reason.
        /// </summary>
        /// <param name="reason">Why this change is canceled.</param>
        public void CancelForExternalReason( string reason )
        {
            if( String.IsNullOrWhiteSpace( reason ) ) throw new ArgumentException();
            if( _externalReasons == null ) _externalReasons = new List<string>();
            _externalReasons.Add( reason );
        }
    }
}
