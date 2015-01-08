#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\PluginAndServiceWrapper\PluginWrapper.cs) is part of CiviKey. 
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

namespace Yodii.Host.Tests
{
    class PluginWrapper
    {
        readonly IYodiiEngine _engine;
        readonly YodiiHost _host;
        readonly string _pluginFullName;
        readonly ServiceWrapper _service;

        public PluginWrapper( Type t, IYodiiEngine engine, YodiiHost host, ServiceWrapper service = null )
        {
            _engine = engine;
            _host = host;
            _pluginFullName = t.FullName;
            _service = service;
            Live = engine.LiveInfo.FindPlugin( _pluginFullName );
            PluginProxy = host.FindLoadedPlugin( _pluginFullName );
            engine.PropertyChanged += engine_PropertyChanged;
        }

        void engine_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            Assert.That( sender == _engine );
            Live = _engine.LiveInfo.FindPlugin( _pluginFullName );
            PluginProxy = _host.FindLoadedPlugin( _pluginFullName );
        }

        public ILivePluginInfo Live { get; private set; }

        public ServiceWrapper Service { get { return _service; } }

        public IPluginProxy PluginProxy  { get; private set; }

        public object Plugin
        {
            get { return PluginProxy != null ? PluginProxy.RealPluginObject : null; }
        }

        public bool CheckState( PluginStatus s )
        {
            Assert.That( (PluginProxy.Status == PluginStatus.Null) == (PluginProxy.RealPluginObject == null), "Plugin '{0}' Status is Null <==> RealPluginObject == null", _pluginFullName );
            Assert.That( PluginProxy.Status, Is.EqualTo( s ), "Plugin '{0}' status must be {1}.", _pluginFullName, s );
            Assert.That( Live.IsRunning, Is.EqualTo( s == PluginStatus.Started ), "Plugin '{0}' live is {1}. It should not!", _pluginFullName, Live.IsRunning ? "running" : "not running" );
            return true;
        }
        
        public PluginWrapper CheckStoppedOrNull()
        {
            Assert.That( (PluginProxy.Status == PluginStatus.Null) == (PluginProxy.RealPluginObject == null), "Plugin '{0}' Status is Null <==> RealPluginObject == null", _pluginFullName );
            Assert.That( PluginProxy.Status, Is.EqualTo( PluginStatus.Stopped ).Or.EqualTo( PluginStatus.Null ), "Plugin '{0}' status is {1}. It must be Stopped or Null.", _pluginFullName, PluginProxy.Status );
            Assert.That( Live.IsRunning, Is.False, "Plugin '{0}' live is running. It should not.", _pluginFullName );
            return this;
        }
    }

    class PluginWrapper<T> : PluginWrapper where T : IYodiiPlugin
    {
        public PluginWrapper( IYodiiEngine engine, YodiiHost host, ServiceWrapper service = null )
            : base( typeof(T), engine, host, service )
        {
        }

        public new T Plugin { get { return (T)base.Plugin; } }
    }

}
