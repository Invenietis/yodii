#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\PluginStatus.cs) is part of CiviKey. 
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
using Yodii.Model;

namespace Yodii.Host
{
    /// <summary>
    /// Defines the status of a plugin with its <see cref="Stopping"/> and <see cref="Starting"/> transitions.
    /// </summary>
    public enum PluginStatus
    {
        /// <summary>
        /// Plugin is not instanciated.
        /// </summary>
        Null = 0,

        /// <summary>
        /// Plugin is stopped.
        /// </summary>
        Stopped = 1,
        
        /// <summary>
        /// Plugin is stopping: <see cref="IYodiiPlugin.PreStop"/> has been called
        ///  but not <see cref="IYodiiPlugin.Stop"/> yet.
        /// </summary>
        Stopping = 2,

        /// <summary>
        /// Plugin is starting: <see cref="IYodiiPlugin.PreStart"/> has been called 
        /// but not <see cref="IYodiiPlugin.Start"/> yet.
        /// </summary>
        Starting = 3,
        
        /// <summary>
        /// Plugin is running.
        /// </summary>
        Started = 4
    }
}
