#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\IYodiiEngineHostApplyResult.cs) is part of CiviKey. 
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
using CK.Core;

namespace Yodii.Model
{

    /// <summary>
    /// Defines the returned result of <see cref="IYodiiEngineHost.Apply"/>.
    /// </summary>
    public interface IYodiiEngineHostApplyResult
    {
        /// <summary>
        /// Gets the errors if any. Never null (empty if none).
        /// </summary>
        IReadOnlyList<IPluginHostApplyCancellationInfo> CancellationInfo { get; }

        /// <summary>
        /// Gets the actions that must be triggered. Never null (empty if none).
        /// </summary>
        IReadOnlyList<Action<IYodiiEngine>> PostStartActions { get; }
    }

}
