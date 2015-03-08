#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Host\Plugin\IYodiiPlugin.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// This interface defines the minimal properties and behavior of a plugin.
    /// It implements a two-phases transition: plugin that should stop or start
    /// can accept or reject the transition thanks to <see cref="PreStop"/> and <see cref="PreStart"/>.
    /// If all of them aggreed, then <see cref="Stop"/> and <see cref="Start"/> are called.
    /// </summary>
    public interface IYodiiPlugin
    {
        /// <summary>
        /// Called before the actual <see cref="Stop"/> method.
        /// Implementations must validate that this plugin can be stoppped: if not, the transition must 
        /// be canceled by calling <see cref="IPreStopContext.Cancel"/>.
        /// </summary>
        /// <param name="c">The context to use.</param>
        void PreStop( IPreStopContext c );

        /// <summary>
        /// Called before the actual <see cref="Start"/> method.
        /// Implementations must validate that the start is possible and, if unable 
        /// to start, cancels it by calling <see cref="IPreStartContext.Cancel"/> .
        /// </summary>
        /// <param name="c">The context to use.</param>
        void PreStart( IPreStartContext c );

        /// <summary>
        /// Called after successful calls to all <see cref="PreStop"/> and <see cref="PreStart"/>.
        /// This may also be called to cancel a previous call to <see cref="PreStart"/> if another
        /// plugin rejected the transition.
        /// </summary>
        /// <param name="c">The context to use.</param>
        void Stop( IStopContext c );

        /// <summary>
        /// Called after successful calls to all <see cref="PreStop"/> and <see cref="PreStart"/>.
        /// This may also be called to cancel a previous call to <see cref="PreStop"/> if another
        /// plugin rejected the transition.
        /// </summary>
        /// <param name="c">The context to use.</param>
        void Start( IStartContext c );
    }
}
