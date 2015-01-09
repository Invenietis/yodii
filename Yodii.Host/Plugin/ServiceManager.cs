#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\ServiceManager.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Host
{
    class ServiceManager
    {
        readonly ServiceHost _serviceHost;
        readonly Dictionary<IServiceInfo,Impact> _services;

        [DebuggerDisplay( "Service={ServiceInfo}, Starting={Starting}, Implementation={Implementation}, SwappedImplementation={SwappedImplementation}, Generalization={ServiceGeneralization}" )]
        public class Impact
        {
            /// <summary>
            /// The service info is the key.
            /// </summary>
            public readonly IServiceInfo ServiceInfo;
                        
            /// <summary>
            /// The service itself. Null if no Service is implemented by PluginToDisable
            /// or Implementation.Plugin.
            /// </summary>
            public readonly ServiceProxyBase Service;
                        
            /// <summary>
            /// The service generalization if it exists.
            /// </summary>
            public Impact ServiceGeneralization;
            
            /// <summary>
            /// When SwappedImplementation is not null, this is the plugin that 
            /// is stopping and Starting is true.
            /// </summary>
            public readonly StContext Implementation;
            
            /// <summary>
            /// If it is not starting, then it is stopping (and SwappedImplementation 
            /// is necessarily null).
            /// </summary>
            public bool Starting;

            /// <summary>
            /// Number of StStopContext for this impact that are not IsDisabledOnly.
            /// It can be 0 or 1: more than one real stop for the same impact (hence the same
            /// Service) means that the host is asked to stop 2 running plugins for the same service...
            /// This is an error in the Engine.
            /// </summary>
            public int NumberOfRealStop;

            /// <summary>
            /// Plugin that replaces the current Implementation.
            /// </summary>
            public StStartContext SwappedImplementation;

            public Impact( ServiceHost serviceHost, IServiceInfo service, bool starting, StContext impl )
            {
                Debug.Assert( service != null );
                Debug.Assert( impl != null || !starting, "impl == null => starting is false." );
                Service = serviceHost.EnsureProxyForDynamicService( ServiceInfo = service );
                Implementation = impl;
                Starting = starting;
            }

            //public override string ToString()
            //{
            //    var s = String.Format( "Starting={0}, Implementation={2}, SwappedImplementation={3}", Starting, Implementation, SwappedImplementation );
            //    return ServiceGeneralization != null ? s + ", Generalization={ " + ServiceGeneralization.ToString() + " }" : s;
            //}
        }

        public ServiceManager( ServiceHost serviceHost )
        {
            _serviceHost = serviceHost;
            _services = new Dictionary<IServiceInfo, Impact>();
        }

        public Impact AddToStop( IServiceInfo s, StStopContext c )
        {
            Impact impact;
            if( !_services.TryGetValue( s, out impact ) )
            {
                impact = new Impact( _serviceHost, s, false, c );
                _services.Add( s, impact );
            }
            if( !c.IsDisabledOnly )
            {
                if( ++impact.NumberOfRealStop > 1 )
                {
                    throw new CKException( R.HostApplyInvalidGeneralizationMismatchStopped );
                }
            }
            if( c.ServiceImpact == null ) c.ServiceImpact = impact;
            if( s.Generalization != null )
            {
                impact.ServiceGeneralization = AddToStop( s.Generalization, c );
            }
            return impact;
        }

        public Impact AddToStart( IServiceInfo s, StStartContext c )
        {
            Impact impact;
            if( _services.TryGetValue( s, out impact ) )
            {
                if( impact.Starting )
                {
                    // A previous Start has been registered for a plugin that implements the same service.
                    throw new CKException( R.HostApplyInvalidGeneralizationMismatchStart );
                }
                Debug.Assert( impact.SwappedImplementation == null );
                impact.Starting = true;
                impact.SwappedImplementation = c;
                impact.Implementation.Status = StContext.StStatus.StoppingSwap;
                // If the Start context has no impact yet, its plugin is replacing another one for the same service.
                // Else, if the ServiceImpact is already associated, then the starting plugin is replacing another one for a more abstract service
                // they have in common.
                if( c.ServiceImpact == null )
                {
                    c.SwappedServiceImpact = c.ServiceImpact = impact;
                }
                else if( c.SwappedServiceImpact == null )
                {
                    c.SwappedServiceImpact = impact;
                }
                Debug.Assert( c.Status == StContext.StStatus.StartingSwap );
            }
            else
            {
                impact = new Impact( _serviceHost, s, true, c );
                if( c.ServiceImpact == null ) c.ServiceImpact = impact;
                _services.Add( s, impact );
            }
            if( s.Generalization != null )
            {
                impact.ServiceGeneralization = AddToStart( s.Generalization, c );
            }
            return impact;
        }
    }

}
