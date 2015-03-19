#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\CancellationInfo.cs) is part of CiviKey. 
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

namespace Yodii.Host
{
    class CancellationInfo : IPluginHostApplyCancellationInfo
    {
        public CancellationInfo( IPluginInfo p, bool isLoadError = false )
        {
            Plugin = p;
            IsLoadError = isLoadError;
        }

        public IPluginInfo Plugin { get; private set; }

        public bool IsLoadError { get; private set; }

        public bool IsPreStartOrStopUnhandledException { get; set; }
        
        public bool IsStartCanceled { get; set; }

        public bool IsStopCanceled { get { return !IsLoadError && !IsStartCanceled; } }

        public string ErrorMessage { get; set; }

        public Exception Error { get; set; }
    }
}
