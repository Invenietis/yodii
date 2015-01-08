#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\ServiceCallException.cs) is part of CiviKey. 
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
    /// Base exception for <see cref="ServiceNotAvailableException"/> and <see cref="ServiceCallBlockedException"/>. 
    /// </summary>
	[Serializable]
	public class ServiceCallException : Exception, ISerializable
	{
        /// <summary>
        /// Gets the service type name.
        /// </summary>
        public string ServiceTypeName { get; private set; }

        /// <summary>
        /// Initializes a new <see cref="ServiceCallException"/>.
        /// </summary>
        /// <param name="serviceType">Type of the concerned service.</param>
		public ServiceCallException( Type serviceType )
		{
            ServiceTypeName = serviceType.AssemblyQualifiedName;
		}

        /// <summary>
        /// Initializes a new <see cref="ServiceCallException"/>.
        /// </summary>
        /// <param name="serviceType">Type of the concerned service.</param>
        /// <param name="message">Detailed message.</param>
        public ServiceCallException( Type serviceType, string message )
			: base( message )
		{
            ServiceTypeName = serviceType.AssemblyQualifiedName;
        }

        /// <summary>
        /// Initializes a new <see cref="ServiceNotAvailableException"/> (serialization).
        /// </summary>
        /// <param name="info">Serialization information.</param>
        /// <param name="context">Serialization context.</param>
        protected ServiceCallException( SerializationInfo info, StreamingContext context )
			: base( info, context )
		{
            ServiceTypeName = info.GetString( "ServiceTypeName" );
		}

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.AddValue( "ServiceTypeName", ServiceTypeName );
        }
    }
}
