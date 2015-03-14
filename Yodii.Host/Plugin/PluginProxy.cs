#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\PluginProxy.cs) is part of CiviKey. 
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
using Yodii.Model;
using System.Reflection;

namespace Yodii.Host
{

    sealed class PluginProxy : PluginProxyBase, IPluginProxy, IYodiiEngineProxy
    {
        readonly IYodiiEngineExternal _engine;
        IActivityMonitor _monitor;
        bool _isRunningLocked;

        public PluginProxy( IYodiiEngineExternal e, IPluginInfo pluginKey )
        {
            _engine = e;
            PluginInfo = pluginKey;
        }

        public IPluginInfo PluginInfo { get; internal set; }

        internal bool TryLoad( ServiceHost serviceHost, Func<IPluginInfo, object[], IYodiiPlugin> pluginCreator )
        {
            if( _monitor == null ) _monitor = new ActivityMonitor( PluginInfo.PluginFullName );
            object[] ctorParameters = new object[ PluginInfo.ConstructorInfo.ParameterCount ];
            foreach( var sRef in PluginInfo.ServiceReferences )
            {
                ctorParameters[sRef.ConstructorParameterIndex] = serviceHost.EnsureProxyForDynamicService( sRef.Reference );
            }
            foreach( var knownParam in PluginInfo.ConstructorInfo.KnownParameters )
            {
                if( knownParam.DescriptiveType == "IYodiiEngine" )
                {
                    ctorParameters[knownParam.ParameterIndex] = this;
                }
                else if( knownParam.DescriptiveType == "IActivityMonitor" )
                {
                    ctorParameters[knownParam.ParameterIndex] = _monitor;
                }
            }
            return TryLoad( serviceHost, () => pluginCreator( PluginInfo, ctorParameters ), PluginInfo.PluginFullName );
        }

        #region IYodiiEngine Members

        IYodiiEngineExternal IYodiiEngineProxy.ExternalEngine
        {
            get { return _engine; }
        }

        IConfigurationManager IYodiiEngineBase.Configuration
        {
            get { return _engine.Configuration; }
        }

        ILiveInfo IYodiiEngineBase.LiveInfo
        {
            get { return _engine.LiveInfo; }
        }

        IYodiiEngineResult IYodiiEngineBase.StartItem( ILiveYodiiItem pluginOrService, StartDependencyImpact impact, string callerKey )
        {
            return _engine.StartItem( pluginOrService, impact, callerKey ?? PluginInfo.PluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StopItem( ILiveYodiiItem pluginOrService, string callerKey )
        {
            return _engine.StopItem( pluginOrService, callerKey ?? PluginInfo.PluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StartPlugin( string pluginFullName, StartDependencyImpact impact, string callerKey )
        {
            return _engine.StartPlugin( pluginFullName, impact, callerKey ?? PluginInfo.PluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StopPlugin( string pluginFullName, string callerKey )
        {
            return _engine.StopPlugin( pluginFullName, callerKey ?? PluginInfo.PluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StartService( string serviceFullName, StartDependencyImpact impact, string callerKey )
        {
            return _engine.StartService( serviceFullName, impact, callerKey ?? PluginInfo.PluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StopService( string serviceFullName, string callerKey )
        {
            return _engine.StopService( serviceFullName, callerKey ?? PluginInfo.PluginFullName );
        }

        #endregion

        public event EventHandler IsRunningLockedChanged;

        public bool IsRunningLocked
        {
            get { return _isRunningLocked; }
        }

        internal void SetRunningLocked( bool value )
        {
            if( _isRunningLocked != value )
            {
                _isRunningLocked = value;
                var h = IsRunningLockedChanged;
                if( h != null ) h( this, EventArgs.Empty );
            }
        }

    }
}
