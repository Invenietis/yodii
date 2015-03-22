#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\ISimpleServiceHostConfiguration.cs) is part of CiviKey. 
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
using System.Reflection;

namespace Yodii.Host
{
    /// <summary>
    /// Extension of the basic <see cref="IServiceHostConfiguration"/> that 
    /// memorizes its configuration and provides helpers to set multiple configurations at once.
    /// </summary>
    public interface ISimpleServiceHostConfiguration : IServiceHostConfiguration
    {
        /// <summary>
        /// Clears all configurations.
        /// </summary>
        void Clear();

        /// <summary>
        /// Configures the behavior on events.
        /// </summary>
        /// <param name="e">The <see cref="EventInfo"/> to configure.</param>
        /// <param name="option">Options to set.</param>
        void SetConfiguration( EventInfo e, ServiceLogEventOptions option );

        /// <summary>
        /// Configures the behavior on method calls.
        /// </summary>
        /// <param name="m">Method to configure.</param>
        /// <param name="option">Options to set.</param>
        void SetConfiguration( MethodInfo m, ServiceLogMethodOptions option );

        /// <summary>
        /// Configures the behavior on property calls.
        /// </summary>
        /// <param name="p">Property to configure.</param>
        /// <param name="option">Options to set.</param>
        void SetConfiguration( PropertyInfo p, ServiceLogMethodOptions option );

        /// <summary>
        /// Helper that configures all events on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="option">Options to set.</param>
        void SetAllEventsConfiguration( Type type, ServiceLogEventOptions option );

        /// <summary>
        /// Helper that configures all methods on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="option">Options to set.</param>
        void SetAllMethodsConfiguration( Type type, ServiceLogMethodOptions option );

        /// <summary>
        /// Helper that configures all properties on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="option">Options to set.</param>
        void SetAllPropertiesConfiguration( Type type, ServiceLogMethodOptions option );

        /// <summary>
        /// Helper that configures all overloads of a method (a method group) on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="option">Options to set.</param>
        void SetMethodGroupConfiguration( Type type, string methodName, ServiceLogMethodOptions option );
    }
}
