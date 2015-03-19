#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Service\SimpleServiceHostConfiguration.cs) is part of CiviKey. 
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
using System.Reflection;
using System.Linq;
using Yodii.Model;


namespace Yodii.Host
{
    /// <summary>
    /// Simple dictionary based implementation of <see cref="IServiceHostConfiguration"/>.
    /// </summary>
    public class SimpleServiceHostConfiguration : IServiceHostConfiguration, ISimpleServiceHostConfiguration
    {
        readonly Dictionary<MethodInfo,ServiceLogMethodOptions> _methods;
        readonly Dictionary<EventInfo,ServiceLogEventOptions> _events;

        /// <summary>
        /// Initializes a new empty configuration.
        /// </summary>
        public SimpleServiceHostConfiguration()
        {
            _methods = new Dictionary<MethodInfo, ServiceLogMethodOptions>();
            _events = new Dictionary<EventInfo, ServiceLogEventOptions>();
        }

        /// <summary>
        /// Clears all configurations.
        /// </summary>
        public void Clear()
        {
            _methods.Clear();
            _events.Clear();
        }

        /// <summary>
        /// Configures the behavior on method calls.
        /// </summary>
        /// <param name="m">Method to configure.</param>
        /// <param name="option">Options to set.</param>
        public void SetConfiguration( MethodInfo m, ServiceLogMethodOptions option )
        {
            _methods[m] = option;
        }

        /// <summary>
        /// Configures the behavior on property calls.
        /// </summary>
        /// <param name="p">Property to configure.</param>
        /// <param name="option">Options to set.</param>
        public void SetConfiguration( PropertyInfo p, ServiceLogMethodOptions option )
        {
            MethodInfo mG = p.GetGetMethod();
            if( mG != null ) SetConfiguration( mG, option );
            MethodInfo mS = p.GetSetMethod();
            if( mS != null ) SetConfiguration( mS, option );
        }

        /// <summary>
        /// Configures the behavior on events.
        /// </summary>
        /// <param name="e">The <see cref="EventInfo"/> to configure.</param>
        /// <param name="option">Options to set.</param>
        public void SetConfiguration( EventInfo e, ServiceLogEventOptions option )
        {
            _events[e] = option;
        }

        /// <summary>
        /// Helper that configures all overloads of a method (a method group) on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="methodName">Name of the method.</param>
        /// <param name="option">Options to set.</param>
        public void SetMethodGroupConfiguration( Type type, string methodName, ServiceLogMethodOptions option )
        {
            foreach( var m in CK.Reflection.ReflectionHelper.GetFlattenMethods( type ).Where( m => m.Name == methodName ) )
            {
                _methods[m] = option;
            }
        }

        /// <summary>
        /// Helper that configures all methods on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="option">Options to set.</param>
        public void SetAllMethodsConfiguration( Type type, ServiceLogMethodOptions option )
        {
            foreach( var m in CK.Reflection.ReflectionHelper.GetFlattenMethods( type ).Where( m => !m.IsSpecialName ) )
            {
                _methods[m] = option;
            }
        }

        /// <summary>
        /// Helper that configures all properties on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="option">Options to set.</param>
        public void SetAllPropertiesConfiguration( Type type, ServiceLogMethodOptions option )
        {
            foreach( var p in CK.Reflection.ReflectionHelper.GetFlattenProperties( type ) ) SetConfiguration( p, option );
        }

        /// <summary>
        /// Helper that configures all events on a service.
        /// </summary>
        /// <param name="type">Type of the service.</param>
        /// <param name="option">Options to set.</param>
        public void SetAllEventsConfiguration( Type type, ServiceLogEventOptions option )
        {
            foreach( var e in CK.Reflection.ReflectionHelper.GetFlattenEvents( type ) )
            {
                _events[e] = option;
            }
        }

        /// <summary>
        /// Returns the <see cref="ServiceLogMethodOptions"/> for the given method.
        /// </summary>
        /// <param name="m">Method for which options should be obtained.</param>
        /// <returns>Configuration for the method.</returns>
        public ServiceLogMethodOptions GetOptions( MethodInfo m )
        {
            ServiceLogMethodOptions o;
            _methods.TryGetValue( m, out o );
            return o;
        }

        /// <summary>
        /// Returns the <see cref="ServiceLogEventOptions"/> for the given event.
        /// </summary>
        /// <param name="e">Event for which options should be obtained.</param>
        /// <returns>Configuration for the event.</returns>
        public ServiceLogEventOptions GetOptions( EventInfo e )
        {
            ServiceLogEventOptions o;
            _events.TryGetValue( e, out o );
            return o;
        }

    }

}
