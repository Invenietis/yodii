#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Host\Plugin\IPreStopContext.cs) is part of CiviKey. 
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
    /// Transition context for <see cref="IYodiiPlugin.PreStop"/>.
    /// </summary>
    public interface IPreStopContext
    {
        /// <summary>
        /// Cancels the stop with an optional exception and/or message.
        /// If for any reason a plugin can not or refuse to stop, this method must be called.
        /// </summary>
        /// <param name="message">Reason to reject the stop.</param>
        /// <param name="ex">Optional exception that occurred.</param>
        void Cancel( string message = null, Exception ex = null );

        /// <summary>
        /// Gets a shared storage that is available during the whole transition.
        /// Used to exchange any kind of information during a transition: this typically 
        /// holds data that enables plugin hot swapping.
        /// </summary>
        IDictionary<object, object> SharedMemory { get; }

        /// <summary>
        /// Gets or sets the action that will be executed if any other PreStop or PreStart fails.
        /// Note that this rollback action will not be called for the plugin that called <see cref="Cancel"/>.
        /// Defaults to <see cref="IYodiiPlugin.Start"/>.
        /// </summary>
        Action<IStartContext> RollbackAction { get; set; }
    }
}
