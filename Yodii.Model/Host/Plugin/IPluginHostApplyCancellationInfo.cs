#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Host\Plugin\IPluginHostApplyCancellationInfo.cs) is part of CiviKey. 
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
    /// Exposes cancellation information when <see cref="IYodiiPlugin.PreStart"/> 
    /// or <see cref="IYodiiPlugin.PreStop"/> failed or refused to start or stop.
    /// </summary>
    public interface IPluginHostApplyCancellationInfo
    {
        /// <summary>
        /// Gets the plugin that failed to start or stop.
        /// </summary>
        IPluginInfo Plugin { get; }

        /// <summary>
        /// Gets whether the <see cref="Error"/> occurred while creating a new plugin instance.
        /// </summary>
        bool IsLoadError { get; }

        /// <summary>
        /// Gets whether the <see cref="Error"/> is an unhandled exception that has been raised by <see cref="IYodiiPlugin.PreStop"/> or <see cref="IYodiiPlugin.PreStart"/>.
        /// Note that such exceptions are handled by Yodii only when <see cref="IYodiiEngineHost.CatchPreStartOrPreStopExceptions"/> is set to true.
        /// When false (that is the default), exceptions in PreStart/Stop bubble up, causing the engine to stop.
        /// </summary>
        bool IsPreStartOrStopUnhandledException { get; }

        /// <summary>
        /// Gets whether it is <see cref="IYodiiPlugin.PreStart"/> that failed.
        /// </summary>
        bool IsStartCanceled { get; }

        /// <summary>
        /// Gets whether it is <see cref="IYodiiPlugin.PreStop"/> that failed.
        /// </summary>
        bool IsStopCanceled { get; }

        /// <summary>
        /// Gets the message.
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets an optional exception that occurred during the operation.
        /// </summary>
        Exception Error { get; }
    }
}
