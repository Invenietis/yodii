#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\LiveModel\ILiveInfo.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Live information of a running engine.
    /// This is the observable façade to the whole <see cref="IYodiiEngine"/>.
    /// </summary>
    /// <remarks>
    /// Gives information about which services and plugins were successfully resolved and started and supports 
    /// start and stop capabilities.
    /// </remarks>
    public interface ILiveInfo
    {
        /// <summary>
        /// Gets an observable list of currently available plugins.
        /// </summary>
        IObservableReadOnlyList<ILivePluginInfo> Plugins { get; }
        
        /// <summary>
        /// Gets an observable list of currently available services.
        /// </summary>
        IObservableReadOnlyList<ILiveServiceInfo> Services { get; }
        
        /// <summary>
        /// Currently active <see cref="YodiiCommand"/>.
        /// </summary>
        /// <remarks>
        /// Commands are are used to dynamically reconfigure the system.
        /// They are memorized from the newest one to the latest one and are automatically optimized: only 
        /// commands that are actually used to preserve the integrity of the system state are kept.
        /// </remarks>
        IObservableReadOnlyList<YodiiCommand> YodiiCommands { get; }
        
        /// <summary>
        /// Find a service by its full name.
        /// </summary>
        /// <param name="serviceFullName">Full name of the live service</param>
        /// <returns>Live service.</returns>
        ILiveServiceInfo FindService( string serviceFullName );
        
        /// <summary>
        /// Find a plugin by its full name.
        /// </summary>
        /// <param name="pluginFullName">Plugin full name.</param>
        /// <returns>Live plugin.</returns>
        ILivePluginInfo FindPlugin( string pluginFullName );

        /// <summary>
        /// Find a service or a plugin by its full name.
        /// </summary>
        /// <param name="pluginOrserviceFullName">Full name of the service or plugin.</param>
        /// <returns>Live service or plugin.</returns>
        ILiveYodiiItem FindYodiiItem( string pluginOrserviceFullName );
        
        /// <summary>
        /// Cancels any start or stop made by the given caller.
        /// </summary>
        /// <param name="callerKey">The caller key that identifies the caller. Null is considered to be the same as <see cref="String.Empty"/>.</param>
        /// <returns>Since canceling commands may trigger a runtime error, this method must return a result.</returns>
        IYodiiEngineResult RevokeCaller( string callerKey = null );

    }

}
