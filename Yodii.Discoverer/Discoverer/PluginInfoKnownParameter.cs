#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Discoverer\Discoverer\PluginInfoKnownParameter.cs) is part of CiviKey. 
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
using Yodii.Model;

namespace Yodii.Discoverer
{
    class PluginInfoKnownParameter : IPluginCtorKnownParameterInfo
    {
        readonly string _parameterName;
        readonly int _parameterIndex;
        readonly string _descriptiveType;

        public PluginInfoKnownParameter(string parameterName, int parameterIndex, string descriptiveType )
        {
            _parameterName = parameterName;
            _parameterIndex = parameterIndex;
            _descriptiveType = descriptiveType;
        }

        public string ParameterName
        {
            get { return _parameterName; }
        }

        public int ParameterIndex
        {
            get { return _parameterIndex; }
        }

        public string DescriptiveType
        {
            get { return _descriptiveType; }
        }
    }
}
