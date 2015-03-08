#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\FinalConfigurationItem.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Read-only configuration item.
    /// </summary>
    public struct FinalConfigurationItem
    {
        readonly string _serviceOrPluginFullName;
        readonly ConfigurationStatus _status;
        readonly StartDependencyImpact _impact;

        /// <summary>
        /// Service or plugin ID.
        /// </summary>
        public string ServiceOrPluginFullName
        {
            get { return _serviceOrPluginFullName; }
        }

        /// <summary>
        /// Required configuration status.
        /// </summary>
        public ConfigurationStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Required configuration impact.
        /// </summary>
        public StartDependencyImpact Impact
        {
            get { return _impact; }
        }

        /// <summary>
        /// A FinalConfigurationItem is an immutable object that displays the latest configuration of an item.
        /// </summary>
        public FinalConfigurationItem( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact = StartDependencyImpact.Unknown )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginFullName ) );

            _serviceOrPluginFullName = serviceOrPluginFullName;
            _status = status;
            _impact = impact;
        }

        /// <summary>
        /// This function encapsulates all configuration constraints regarding the <see cref="ConfigurationStatus"/> of an item.
        /// </summary>
        /// <param name="s1">First <see cref="ConfigurationStatus"/>.</param>
        /// <param name="s2">Second <see cref="ConfigurationStatus"/>.</param>
        /// <param name="invalidCombination">string expliciting the error. Empty if all is well</param>
        /// <returns>Resulting combined status.</returns>
        public static ConfigurationStatus Combine( ConfigurationStatus s1, ConfigurationStatus s2, out string invalidCombination )
        {
            invalidCombination = "";
            if( s1 == s2 ) return s1;
           
            if( s2 != ConfigurationStatus.Optional )
            {
                if( s1 == ConfigurationStatus.Optional || (s1 >= ConfigurationStatus.Runnable && s2 >= ConfigurationStatus.Runnable) )
                {
                    return (s1 == ConfigurationStatus.Running) ? s1 : s2;
                }
                else if( s1 != s2 )
                {
                    invalidCombination = string.Format( "Conflict for statuses {0} and {1}", s1, s2 );
                    return s1;
                }
                else
                {
                    bool invalidS1 = !Enum.IsDefined( typeof( ConfigurationStatus ), s1 );
                    Debug.Assert( invalidS1 || !Enum.IsDefined( typeof( ConfigurationStatus ), s2 ) );
                    invalidCombination = string.Format( "Invalid status value: {0}", invalidS1 ? (int)s1 : (int)s2 );
                    return s1;
                }
            }
            return s1;
        }
    }
}
