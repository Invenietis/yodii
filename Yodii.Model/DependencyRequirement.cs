#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\DependencyRequirement.cs) is part of CiviKey. 
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
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Describes how a plugin requires a service. 
    /// A requirement is a gradation between <see cref="Optional"/> and <see cref="Running"/>.
    /// </summary>
    [Flags]
    public enum DependencyRequirement
    {
        /// <summary>
        /// The service is optional: it can be unavailable.
        /// </summary>
        Optional = 0,

        /// <summary>
        /// If the service is available, it is better if it is started (it is a "recommended" service).
        /// </summary>
        OptionalRecommended = 1,

        /// <summary>
        /// The service must be available (ready to run but it can be stopped if nothing else want to start it).
        /// It is guaranteed to be runnable.
        /// </summary>
        Runnable = 4,

        /// <summary>
        /// The service must be available and it is better if it is started (it is a "recommended" service). 
        /// It can always be stopped at any time.
        /// </summary>
        RunnableRecommended = 5,

        /// <summary>
        /// The service must be running.
        /// </summary>
        Running = 2 + 4
    }
}
