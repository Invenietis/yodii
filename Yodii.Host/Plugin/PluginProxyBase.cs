#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\PluginProxyBase.cs) is part of CiviKey. 
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
using System.Diagnostics;
using CK.Core;
using System.Reflection;
using Yodii.Model;


namespace Yodii.Host
{
    class PluginProxyBase
    {
        IYodiiPlugin _instance;
        Exception  _loadError;

        public PluginStatus Status { get; set; }

        /// <summary>
        /// Gets the implemented service.
        /// </summary>
        internal ServiceProxyBase Service { get; private set; }

        public Exception LoadError 
        { 
            get 
            {
                Debug.Assert( (_instance == null) == (Status == PluginStatus.Null), "_instance == null <==> Status == Null" );
                return _loadError; 
            } 
        }

        public IYodiiPlugin RealPluginObject
        {
            get
            {
                Debug.Assert( (_instance == null) == (Status == PluginStatus.Null), "_instance == null <==> Status == Null" );
                return _instance;
            }
        }

        internal MethodInfo GetImplMethodInfoPreStop() { return GetImplMethodInfo( typeof( IYodiiPlugin ), "PreStop" ); }

        internal MethodInfo GetImplMethodInfoPreStart() { return GetImplMethodInfo( typeof( IYodiiPlugin ), "PreStart" ); }

        internal MethodInfo GetImplMethodInfoStop() { return GetImplMethodInfo( typeof( IYodiiPlugin ), "Stop" ); }

        internal MethodInfo GetImplMethodInfoStart() { return GetImplMethodInfo( typeof( IYodiiPlugin ), "Start" ); }
        
        internal MethodInfo GetImplMethodInfoDispose() { return GetImplMethodInfo( typeof( IDisposable ), "Dispose" ); }

        MethodInfo GetImplMethodInfo( Type interfaceType, string methodName ) 
        {
            Debug.Assert( RealPluginObject != null );
            MethodInfo m = RealPluginObject.GetType().GetMethod( interfaceType.FullName + '.' + methodName );
            if( m == null ) m = RealPluginObject.GetType().GetMethod( methodName, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public );
            return m;
        }

        /// <summary>
        /// Supports <see cref="IDisposable"/> implementation and sets Status to Null.
        /// If the real plugin does not implement IDisposable, nothing is done and 
        /// the current reference instance is kept (it will be reused and the Status stays to Stopped).
        /// If IDisposable is implemented, a call to Dispose may throw an exception (it is routed to the ServiceHost.LogMethodError), but the _instance 
        /// reference is set to null: a new object will always have to be created if the plugin needs to be started again.
        /// </summary>
        internal void Disable( IActivityMonitor m, bool setToNull = false )
        {
            Debug.Assert( Status == PluginStatus.Stopped, "Status has been set to Stopped." );
            IDisposable di = _instance as IDisposable;
            try
            {
                if( di != null ) di.Dispose();
            }
            catch( Exception ex )
            {
                m.Error().Send( ex );
            }
            finally
            {
                // Clear _instance after Dispose: if an exception is raised,
                // GetImplMethodInfoDispose() may be called by the logger.
                if( setToNull || di != null )
                {
                    Status = PluginStatus.Null;
                    _instance = null;
                }
            }
        }

        internal bool TryLoad( ServiceHost serviceHost, Func<IYodiiPlugin> pluginCreator, object pluginKey )
        {
            if( _instance == null )
            {
                try
                {
                    _instance = pluginCreator();
                    if( _instance == null )
                    {
                        _loadError = new CKException( R.PluginCreatorReturnedNull, pluginKey );
                        return false;
                    }
                    Type t = _instance.GetType();
                    if( typeof( IYodiiService ).IsAssignableFrom( t ) )
                    {
                        Type iType = t.GetInterfaces().FirstOrDefault( i => i != typeof( IYodiiService ) && typeof( IYodiiService ).IsAssignableFrom( i ) );
                        if( iType != null )
                        {
                            Service = serviceHost.EnsureProxyForDynamicService( iType );
                        }
                    }
                }
                catch( Exception ex )
                {
                    _loadError = ex;
                    return false;
                }
            }
            return true;
        }

    }
}
