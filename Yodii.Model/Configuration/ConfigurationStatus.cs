#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\ConfigurationStatus.cs) is part of CiviKey. 
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
    /// Configuration status for a configuration item (a plugin or a service).
    /// </summary>
    public enum ConfigurationStatus
    {
        /// <summary>
        /// Item must be disabled, and cannot be running.
        /// </summary>
        Disabled = -1,

        /// <summary>
        /// Item can be running or not. This is the default.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// Item must be able to start as needed.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// Item must be running.
        /// </summary>
        Running = 6
    }
}
