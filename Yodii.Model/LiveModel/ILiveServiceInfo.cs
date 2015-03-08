#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\LiveModel\ILiveServiceInfo.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Live status of a service, when the engine is started.
    /// </summary>
    public interface ILiveServiceInfo : ILiveYodiiItem, IDynamicSolvedService, INotifyPropertyChanged
    {
        /// <summary>
        /// Live generalization, if this service has one.
        /// </summary>
        ILiveServiceInfo Generalization { get; }

        /// <summary>
        /// Running plugin that implements this service when started.
        /// </summary>
        ILivePluginInfo RunningPlugin { get; }

        /// <summary>
        /// Last running plugin.
        /// </summary>
        ILivePluginInfo LastRunningPlugin { get; }

    }
}
