#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Graph elements\YodiiGraphEdgeType.cs) is part of CiviKey. 
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

namespace Yodii.Lab
{
    /// <summary>
    /// Relationship between two elements in the Yodii Lab graph.
    /// </summary>
    public enum YodiiGraphEdgeType
    {
        /// <summary>
        /// Plugin implementing target service
        /// </summary>
        Implementation = 0,
        /// <summary>
        /// Service specializing target service
        /// </summary>
        Specialization = 1,
        /// <summary>
        /// Plugin referencing (through requirements) a service
        /// </summary>
        ServiceReference = 2
    }
}
