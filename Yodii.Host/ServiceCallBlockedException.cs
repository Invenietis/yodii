#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\ServiceCallBlockedException.cs) is part of CiviKey. 
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
using System.Runtime.Serialization;

namespace Yodii.Host
{

    /// <summary>
    /// Exception raised when calling a service when calls to services are not allowed:
    /// when loading a plugin (from its constructor) or when executing PreStart or Stop method.
    /// </summary>
	[Serializable]
    public class ServiceCallBlockedException : ServiceCallException
	{
        /// <summary>
        /// Initializes a new <see cref="ServiceCallBlockedException"/>.
        /// </summary>
        /// <param name="calledServiceType">Type of the called service.</param>
        /// <param name="message">Detailed message.</param>
        public ServiceCallBlockedException( Type calledServiceType, string message )
			: base( calledServiceType, message )
		{
        }

    }
}
