#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\IServiceHost.cs) is part of CiviKey. 
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
using System.Text;
using Yodii.Model;

namespace Yodii.Host
{
    /// <summary>
    /// Host for <see cref="IYodiiService"/> management.
    /// </summary>
    public interface IServiceHost
    {
        /// <summary>
        /// Gets a <see cref="ISimpleServiceHostConfiguration"/> that is always taken into account (one can not <see cref="Remove"/> it).
        /// Any change to it must be followed by a call to <see cref="ApplyConfiguration"/>.
        /// </summary>
        ISimpleServiceHostConfiguration DefaultConfiguration { get; }

        /// <summary>
        /// Adds a configuration layer.
        /// The <see cref="ApplyConfiguration"/> must be called to actually update the 
        /// internal configuration.
        /// </summary>
        void Add( IServiceHostConfiguration configurator );

        /// <summary>
        /// Removes a configuration layer.
        /// The <see cref="ApplyConfiguration"/> must be called to actually update the 
        /// internal configuration.
        /// </summary>
        void Remove( IServiceHostConfiguration configurator );

        /// <summary>
        /// Applies the configuration: the <see cref="IServiceHostConfiguration"/> that have been <see cref="Add"/>ed are challenged
        /// for each intercepted method or event.
        /// </summary>
        void ApplyConfiguration();

        /// <summary>
        /// Gets the service implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the service (it can be a wrapped <see cref="IService{T}"/>).</param>
        /// <returns>The implementation or null if it does not exist.</returns>
        IServiceUntyped GetProxy( Type interfaceType );

        /// <summary>
        /// Ensures that a proxy exists for the given interface and associates it to an implementation.
        /// </summary>
        /// <param name="interfaceType">Type of the interface.</param>
        /// <param name="currentImplementation">Implementation to use.</param>
        /// <returns>The proxy object.</returns>
        IServiceUntyped InjectExternalService( Type interfaceType, object currentImplementation );

        /// <summary>
        /// Ensures that a proxy exists for the given <see cref="IYodiiService"/> interface.
        /// </summary>
        /// <param name="interfaceType">Type of the interface that must extend <see cref="IYodiiService"/>.</param>
        /// <returns>The proxy object.</returns>
        IServiceUntyped EnsureProxyForDynamicService( Type interfaceType );

        /// <summary>
        /// Ensures that a proxy exists for a dynamic service. The <see cref="IService{T}"/>.
        /// </summary>
        /// <param name="interfaceType">Type of the service (it can be a wrapped <see cref="IService{T}"/>).</param>
        /// <returns>The proxy to the service or null if it does not exist.</returns>
        IService<T> EnsureProxyForDynamicService<T>() where T : IYodiiService;

    }
}
