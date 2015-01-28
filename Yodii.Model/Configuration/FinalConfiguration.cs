#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\FinalConfiguration.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using CK.Core;

namespace Yodii.Model
{
    /// <summary>
    /// Resolved, read-only configuration.
    /// </summary>
    public class FinalConfiguration
    {
        readonly CKSortedArrayKeyList<FinalConfigurationItem, string> _items;

        /// <summary>
        /// Items of this final configuration.
        /// </summary>
        public IReadOnlyList<FinalConfigurationItem> Items
        {
            get { return _items; }
        }

        /// <summary>
        /// Gets the final configuration status for a given service or plugin.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin to check.</param>
        /// <returns>The status of the item, or Optional if the item does not exist.</returns>
        public ConfigurationStatus GetStatus( string serviceOrPluginFullName )
        {
            Debug.Assert( ConfigurationStatus.Optional == 0 );
            return _items.GetByKey( serviceOrPluginFullName ).Status;
        }

        /// <summary>
        /// Gets the final configuration impact for a given service or plugin.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin to check.</param>
        /// <returns>The impact of the item.</returns>
        public StartDependencyImpact GetImpact( string serviceOrPluginFullName )
        {
            Debug.Assert( StartDependencyImpact.Unknown == 0 );
            return _items.GetByKey( serviceOrPluginFullName ).Impact;
        }

        /// <summary>
        /// Gets the final configuration for a given service or plugin.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin to check.</param>
        /// <returns>The final configuration of the item.</returns>
        public FinalConfigurationItem GetFinalConfiguration( string serviceOrPluginFullName )
        {
            return _items.GetByKey( serviceOrPluginFullName );
        }

        /// <summary>
        /// Creates a new instance of FinalConfiguration, using given statuses.
        /// </summary>
        /// <param name="finalStatusAndImpact">Statuses to set.</param>
        public FinalConfiguration( Dictionary<string, FinalConfigurationItem> finalStatusAndImpact )
            : this()
        {
            foreach( var item in finalStatusAndImpact ) _items.Add( new FinalConfigurationItem(item.Key, item.Value.Status, item.Value.Impact) );
        }

        /// <summary>
        /// Creates a new, empty FinalConfiguration.
        /// </summary>
        public FinalConfiguration()
        {
            _items = new CKSortedArrayKeyList<FinalConfigurationItem, string>( e => e.ServiceOrPluginFullName, StringComparer.Ordinal.Compare );
        }
    }
}
