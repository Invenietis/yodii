#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Configuration\ConfigurationExtensions.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Extends configuration objects with useful static methods.
    /// </summary>
    public static class ConfigurationExtensions
    {
        /// <summary>
        /// Gets whether this <see cref="StartDependencyImpact"/> has at least one IsTryXXX bit sets.
        /// </summary>
        /// <param name="this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>True if a IsTryXXX bit is set.</returns>
        public static bool HasTryBit( this StartDependencyImpact @this )
        {
            return (@this & StartDependencyImpact.TryFullStart) != 0;
        }

        /// <summary>
        /// Returns a <see cref="StartDependencyImpact"/> without any IsTryXXX bit in it.
        /// Note that <see cref="StartDependencyImpact.Minimal"/> is preserved.
        /// </summary>
        /// <param name="this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>Impact without IsTryXXX bits.</returns>
        public static StartDependencyImpact ClearAllTryBits( this StartDependencyImpact @this )
        {
            return @this & (StartDependencyImpact.FullStart|StartDependencyImpact.Minimal);
        }

        /// <summary>
        /// Returns a <see cref="StartDependencyImpact"/> with only IsTryXXX bit in it.
        /// Note that <see cref="StartDependencyImpact.Minimal"/> is always set.
        /// </summary>
        /// <param name="this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>Impact with only IsTryXXX bits.</returns>
        public static StartDependencyImpact ToTryBits( this StartDependencyImpact @this )
        {
            int moved = (int)(@this & StartDependencyImpact.FullStart) << 4;
            int cleared = (int)(@this & StartDependencyImpact.TryFullStart);
            return (StartDependencyImpact)(cleared | moved) | StartDependencyImpact.Minimal;
        }

        /// <summary>
        /// Returns a <see cref="StartDependencyImpact"/> without any IsTryXXX bit that have a XXX bit set.
        /// </summary>
        /// <param name="this">This <see cref="StartDependencyImpact"/>.</param>
        /// <returns>Impact without superfluous IsTryXXX bits.</returns>
        public static StartDependencyImpact ClearUselessTryBits( this StartDependencyImpact @this )
        {
            var noTry = @this & StartDependencyImpact.FullStart;
            var tryMask = (StartDependencyImpact)~((int)noTry << 4);
            return (noTry & tryMask) | (@this & StartDependencyImpact.Minimal);
        }


    }
}
