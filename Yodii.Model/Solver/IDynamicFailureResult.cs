#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Solver\IDynamicFailureResult.cs) is part of CiviKey. 
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
    /// Failure result during application of dynamic resolution on <see cref="IYodiiEngineHost"/>.
    /// <seealso cref="IYodiiEngineHost.Apply( IEnumerable{IPluginInfo}, IEnumerable{IPluginInfo}, IEnumerable{IPluginInfo})"/>
    /// </summary>
    public interface IDynamicFailureResult
    {
        /// <summary>
        /// Dynamic resolution configuration that was attempted.
        /// </summary>
        IDynamicSolvedConfiguration SolvedConfiguration { get; }

        /// <summary>
        /// List of plugin runtime errors causing this failure.
        /// </summary>
        IReadOnlyList<PluginRuntimeError> ErrorPlugins { get; }
    }
}
