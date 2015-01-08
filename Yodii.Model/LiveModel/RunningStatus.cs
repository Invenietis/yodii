#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\LiveModel\RunningStatus.cs) is part of CiviKey. 
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
    /// Defines the current status of a plugin (or a service).
    /// </summary>
    public enum RunningStatus
    {
        /// <summary>
        /// The plugin (or service) is disabled.
        /// </summary>
        Disabled,

        /// <summary>
        /// The plugin (or service) is stopped. It can be started.
        /// </summary>
        Stopped,

        /// <summary>
        /// The plugin (or service) is running. It can be stopped. 
        /// </summary>
        Running,

        /// <summary>
        /// The plugin (or service) is running and cannot be stopped. 
        /// </summary>
        RunningLocked
    }
}
