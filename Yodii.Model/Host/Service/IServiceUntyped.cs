#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\Host\Service\IServiceUntyped.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// This interface (automatically implemented for each <see cref="IYodiiService"/>) is the 
    /// base, non generic type, of <see cref="IService{T}"/>.
    /// </summary>
    public interface IServiceUntyped
    {
        /// <summary>
        /// Gets the service itself.
        /// </summary>
        IYodiiService Service { get; }

        /// <summary>
        /// Gets the current <see cref="ServiceStatus"/> of the service.
        /// </summary>
        ServiceStatus Status { get; }

        /// <summary>
        /// Fires whenever the <see cref="Status"/> changed.
        /// </summary>
        event EventHandler<ServiceStatusChangedEventArgs> ServiceStatusChanged;
    }
}
