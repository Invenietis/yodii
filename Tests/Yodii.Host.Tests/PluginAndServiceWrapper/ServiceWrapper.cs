#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\PluginAndServiceWrapper\ServiceWrapper.cs) is part of CiviKey. 
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
using NUnit.Framework;
using Yodii.Model;
using CK.Core;

namespace Yodii.Host.Tests
{
    abstract class ServiceWrapper
    {
        readonly IYodiiEngineExternal _engine;
        readonly YodiiHost _host;
        readonly string _serviceName;
        readonly List<ServiceStatus> _events;

        protected ServiceWrapper( Type t, IYodiiEngineExternal engine, YodiiHost host )
        {
            _engine = engine;
            _host = host;
            _events = new List<ServiceStatus>();
            _serviceName = t.FullName;
            Service = host.ServiceHost.EnsureProxyForDynamicService( t );
            Service.ServiceStatusChanged += Service_ServiceStatusChanged;
            Live = engine.LiveInfo.FindService( _serviceName );
            engine.PropertyChanged += engine_PropertyChanged;
        }

        void engine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            Assert.That( sender == _engine );
            Live = _engine.LiveInfo.FindService( _serviceName );
        }

        void Service_ServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            _events.Add( Service.Status );
        }

        public IReadOnlyList<ServiceStatus> Events { get { return _events.AsReadOnlyList(); } }

        public void ClearEvents()
        {
            _events.Clear();
        }

        public ILiveServiceInfo Live { get; private set; }

        public IServiceUntyped Service { get; private set; }

        public bool CheckState( ServiceStatus s )
        {
            Assert.That( Service.Status, Is.EqualTo( s ), "Service '{0}' status is {1}.", _serviceName, s );
            Assert.That( Live.IsRunning, Is.EqualTo( s.IsStarted() ), "Service '{0}' live is {1}.", _serviceName, Live.IsRunning ? "running" : "not running" );
            return true;
        }

        public void CheckEventsAndClear( params ServiceStatus[] statuses )
        {
            Events.CheckEvents( statuses );
            ClearEvents();
        }
    }

    class ServiceWrapper<T> : ServiceWrapper where T : IYodiiService
    {
        public ServiceWrapper( IYodiiEngineExternal engine, YodiiHost host )
            : base( typeof(T), engine, host )
        {
        }

        public new IService<T> Service 
        { 
            get { return (IService<T>)base.Service; } 
        }

    }

}
