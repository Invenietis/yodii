#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\YodiiHost.cs) is part of CiviKey. 
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

using CK.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Yodii.Model;
using System.Reflection;
using System.Text;

namespace Yodii.Host
{
    public partial class YodiiHost : IYodiiEngineHost
    {
        class HardStopContext : IPreStopContext, IStopContext
        {
            public HardStopContext( IDictionary<object, object> sharedMemory, bool cancellingPreStart )
            {
                SharedMemory = sharedMemory;
                CancellingPreStart = cancellingPreStart;
            }

            public bool IsCancellable { get { return false; } }

            public void Cancel( string message = null, Exception ex = null )
            {
                // Avoids throwing an exception here. This breaks the contract of the IPreXXXContext
                // but since we are hard stopping and already in emergency state, this is clearly not an issue
                // and avoids unnessecary exceptions.
            }

            public IDictionary<object, object> SharedMemory { get; private set; }

            public Action<IStartContext> RollbackAction { get; set; }
            
            public bool CancellingPreStart { get; set; }

            public bool HotSwapping { get { return false; } }
        }

        /// <summary>
        /// Hadrly stops all plugins.
        /// </summary>
        /// <param name="sharedMemory"></param>
        public void HardStop( Dictionary<object, object> sharedMemory )
        {
            using( _monitor.OpenInfo().Send( "Hard stopping all plugins." ) )
            {
                HardStopContext commonStarted = new HardStopContext( sharedMemory, false );
                HardStopContext commonStarting = new HardStopContext( sharedMemory, true );
                foreach( PluginProxy p in _plugins.Values )
                {
                    if( p.Status == PluginStatus.Started )
                    {
                        try
                        {
                            p.RealPluginObject.PreStop( commonStarted );
                        }
                        catch( Exception ex )
                        {
                            _monitor.Error().Send( ex );
                        }
                        p.Status = PluginStatus.Stopping;
                    }
                    else if( p.Status == PluginStatus.Starting )
                    {
                        try
                        {
                            p.RealPluginObject.Stop( commonStarting );
                        }
                        catch( Exception ex )
                        {
                            _monitor.Error().Send( ex );
                        }
                        p.Status = PluginStatus.Stopped;
                    }
                }
                foreach( PluginProxy p in _plugins.Values )
                {
                    if( p.Status == PluginStatus.Stopping )
                    {
                        try
                        {
                            p.RealPluginObject.Stop( commonStarted );
                        }
                        catch( Exception ex )
                        {
                            _monitor.Error().Send( ex );
                        }
                        p.Status = PluginStatus.Stopped;
                        p.Disable( _monitor, setToNull: true );
                    }
                }
                _serviceHost.HardStop();
            }
        }

    }
}
