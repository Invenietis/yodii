#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Service\DefaultProxyDefinition.cs) is part of CiviKey. 
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
using System.Text;
using System.Reflection;
using System.Diagnostics;
using Yodii.Model;

namespace Yodii.Host
{
	internal class DefaultProxyDefinition : IProxyDefinition
	{
		Type _typeInterface;
        CatchExceptionGeneration _errorCatch;
        bool _isDynamicService;

        public DefaultProxyDefinition( Type typeInterface, CatchExceptionGeneration errorCatch )
		{
			if( !typeInterface.IsInterface )
			{
				throw new ArgumentException( R.TypeMustBeAnInterface, "typeInterface" );
			}
			_typeInterface = typeInterface;
            _errorCatch = errorCatch;
            _isDynamicService = typeof( IYodiiService ).IsAssignableFrom( typeInterface );
        }

        public Type TypeInterface
        {
            get { return _typeInterface; }
        }

        public bool IsDynamicService
        {
            get { return _isDynamicService; }
        }

        public Type ProxyBase
		{
			get { return typeof( ServiceProxyBase ); }
		}

        public ProxyOptions GetEventOptions( EventInfo e )
        {
            ProxyOptions opt = new ProxyOptions();
            
            opt.CatchExceptions = _errorCatch == CatchExceptionGeneration.Always 
                || (_errorCatch == CatchExceptionGeneration.HonorIgnoreExceptionAttribute 
                    && !e.IsDefined( typeof( IgnoreExceptionAttribute ), false ));

            if( _isDynamicService )
            {
                bool stopAllowed = e.IsDefined( typeof( IgnoreServiceStoppedAttribute ), false );
                opt.RuntimeCheckStatus = stopAllowed ? ProxyOptions.CheckStatus.NotDisabled : ProxyOptions.CheckStatus.Running;
            }
            else opt.RuntimeCheckStatus = ProxyOptions.CheckStatus.None;

            return opt;
        }

        public ProxyOptions GetPropertyMethodOptions( PropertyInfo p, MethodInfo m )
        {
            ProxyOptions opt = new ProxyOptions();
            
            opt.CatchExceptions = _errorCatch == CatchExceptionGeneration.Always 
                || (_errorCatch == CatchExceptionGeneration.HonorIgnoreExceptionAttribute 
                    && !(p.IsDefined( typeof( IgnoreExceptionAttribute ), false ) || m.IsDefined( typeof( IgnoreExceptionAttribute ), false )) );

            if( _isDynamicService )
            {
                bool stopAllowed = p.IsDefined( typeof( IgnoreServiceStoppedAttribute ), false ) || m.IsDefined( typeof( IgnoreServiceStoppedAttribute ), false );
                opt.RuntimeCheckStatus = stopAllowed ? ProxyOptions.CheckStatus.NotDisabled : ProxyOptions.CheckStatus.Running;
            }
            else opt.RuntimeCheckStatus = ProxyOptions.CheckStatus.None;
            return opt;
        }

        public ProxyOptions GetMethodOptions( MethodInfo m )
        {
            ProxyOptions opt = new ProxyOptions();
            
            opt.CatchExceptions = _errorCatch == CatchExceptionGeneration.Always
                || (_errorCatch == CatchExceptionGeneration.HonorIgnoreExceptionAttribute
                    && !m.IsDefined( typeof( IgnoreExceptionAttribute ), false ));

            if( _isDynamicService )
            {
                bool stopAllowed = m.IsDefined( typeof( IgnoreServiceStoppedAttribute ), false );
                opt.RuntimeCheckStatus = stopAllowed ? ProxyOptions.CheckStatus.NotDisabled : ProxyOptions.CheckStatus.Running;
            }
            else opt.RuntimeCheckStatus = ProxyOptions.CheckStatus.None;
            return opt;
        }

    }
}
