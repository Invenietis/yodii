#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Utils\DetailedOperationResult.cs) is part of CiviKey. 
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

namespace Yodii.Lab.Utils
{
    /// <summary>
    /// Operation result (success or failure), with a descriptive string.
    /// </summary>
    public class DetailedOperationResult
    {
        /// <summary>
        /// Description of the reason behind the operation success/failure.
        /// </summary>
        public string Reason { get; private set; }

        /// <summary>
        /// Whather the operation was successful.
        /// </summary>
        public bool IsSuccessful { get; private set; }

        /// <summary>
        /// Creates a new instance of DetailedOperationResult.
        /// </summary>
        /// <param name="isSuccessful">Whether the operation was successful.</param>
        /// <param name="reason">Reason behind the operation.</param>
        public DetailedOperationResult( bool isSuccessful = true, string reason = "" )
        {
            Reason = reason;
            IsSuccessful = isSuccessful;
        }

        /// <summary>
        /// Implicit bool operator.
        /// </summary>
        /// <param name="result">object to consider</param>
        /// <returns>True if result is successful.</returns>
        public static implicit operator bool( DetailedOperationResult result )
        {
            return result.IsSuccessful;
        }
    }
}
