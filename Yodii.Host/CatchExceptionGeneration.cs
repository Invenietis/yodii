#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\CatchExceptionGeneration.cs) is part of CiviKey. 
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

namespace Yodii.Host
{
    /// <summary>
    /// Defines how Service proxies handle exceptions on method, property or event of <see cref="IYodiiService"/> interfaces. 
    /// This drives code generation and defaults to <see cref="HonorIgnoreExceptionAttribute"/>.
    /// </summary>
    public enum CatchExceptionGeneration
    {
        /// <summary>
        /// Never generate code that intercepts exceptions.
        /// </summary>
        Never,

        /// <summary>
        /// Always generate code that intercepts exceptions.
        /// </summary>
        Always,

        /// <summary>
        /// Generate code that intercepts exceptions except if the method, property or event of 
        /// the <see cref="IYodiiService"/> interface is marked with <see cref="IgnoreExceptionAttribute"/>.
        /// </summary>
        HonorIgnoreExceptionAttribute
    }
}
