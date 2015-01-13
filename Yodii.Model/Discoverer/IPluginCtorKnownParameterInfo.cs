#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Discoverer\IPluginInfo.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// Description of known parameters: instead of exposing actual type information
    /// with potential parameter attributes, this is a simplified approach that relies on 
    /// a mere <see cref="DescriptiveType"/> that can be used to express condensed, simple information.
    /// For instance, the "IActivityMonitor" or "IYodiiEngine" is enough to handle the injection of those 
    /// two objects whenever a plugin is instanciated.
    /// </summary>
    public interface IPluginCtorKnownParameterInfo
    {
        /// <summary>
        /// Gets the parameter name.
        /// </summary>
        string ParameterName { get; }

        /// <summary>
        /// Gets the parameter index.
        /// </summary>
        int ParameterIndex { get; }

        /// <summary>
        /// Gets a description of the parameter that must identify the parameter 
        /// in terms of type: this must be enough to build/acquire/bind an actual object.
        /// Basic defined types are: "IActivityMonitor" and "IYodiiEngine". 
        /// </summary>
        string DescriptiveType { get; }
    }
}
