#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Live\LiveInfo.cs) is part of CiviKey. 
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
using Yodii.Model;
using CK.Core;
using System.ComponentModel;
using System.Diagnostics;

namespace Yodii.Engine
{
    class LiveInfo : ILiveInfo
    {
        readonly YodiiEngine _engine;
        readonly CKObservableSortedArrayKeyList<LivePluginInfo,string> _plugins;
        readonly CKObservableSortedArrayKeyList<LiveServiceInfo,string> _services;

        internal LiveInfo(YodiiEngine engine)
        {
            Debug.Assert( engine != null );
            _engine = engine;
            _plugins = new CKObservableSortedArrayKeyList<LivePluginInfo, string>( l => l.PluginInfo.PluginFullName );
            _services = new CKObservableSortedArrayKeyList<LiveServiceInfo, string>( l => l.ServiceInfo.ServiceFullName );
        }

        public IObservableReadOnlyList<YodiiCommand> YodiiCommands 
        {
            get { return _engine.YodiiCommands; } 
        }

        public IObservableReadOnlyList<ILivePluginInfo> Plugins
        {
            get { return _plugins; }
        }

        public IObservableReadOnlyList<ILiveServiceInfo> Services
        {
            get { return _services; }
        }

        public ILiveServiceInfo FindService( string serviceFullName )
        {
            if( serviceFullName == null ) throw new ArgumentNullException( "serviceFullName" );
            return _services.GetByKey( serviceFullName );
        }

        public ILivePluginInfo FindPlugin( string pluginFullName )
        {
            if( pluginFullName == null ) throw new ArgumentNullException( "pluginFullName" );
            return _plugins.GetByKey( pluginFullName );
        }

        public ILiveYodiiItem FindYodiiItem( string pluginOrserviceFullName )
        {
            if( pluginOrserviceFullName == null ) throw new ArgumentNullException( "pluginOrserviceFullName" );
            return (ILiveYodiiItem)_plugins.GetByKey( pluginOrserviceFullName ) ?? _services.GetByKey( pluginOrserviceFullName );
        }

        public bool Contains( string serviceFullName )
        {
            return _services.Contains( serviceFullName );
        }

        internal void UpdateFrom( IConfigurationSolver solver )
        {
            // 1 - Removes existing items from live info that do not exist anymore in the new running context.
            //     This raises Collection "item removed" events.
            //
            _services.RemoveWhereAndReturnsRemoved( s => solver.FindService( s.ServiceInfo.ServiceFullName ) == null ).Count();
            _plugins.RemoveWhereAndReturnsRemoved( p => solver.FindPlugin( p.PluginInfo.PluginFullName ) == null ).Count();

            DelayedPropertyNotification notifier = new DelayedPropertyNotification();

            // 2 - Builds two lists of new Services and new Plugins and for already existing ones,
            //     updates them with the new information.
            //     This update does not trigger any ProprtyChanged events and consider only 
            //     direct properties of the object.
            //     Changes to linked items (such as a Generalization reference for instance will be 
            //     done later thanks to their Bind method.
            //
            List<LiveServiceInfo> servicesToAdd = new List<LiveServiceInfo>();
            foreach( var s in solver.AllServices )
            {
                LiveServiceInfo ls = _services.GetByKey( s.ServiceInfo.ServiceFullName );
                if( ls == null ) servicesToAdd.Add( new LiveServiceInfo( s, _engine ) );
                else ls.UpdateFrom( s, notifier );
            }

            List<LivePluginInfo> pluginsToAdd = new List<LivePluginInfo>();
            foreach( var p in solver.AllPlugins )
            {
                LivePluginInfo lp = _plugins.GetByKey( p.PluginInfo.PluginFullName );
                if( lp == null ) pluginsToAdd.Add( new LivePluginInfo( p, _engine ) );
                else lp.UpdateFrom( p, notifier );
            }

            // 3 - Intrinsic properties have been updated. We now consider the properties that reference other items.
            //
            Func<string,LiveServiceInfo> serviceFinder = name => _services.GetByKey( name ) ?? servicesToAdd.First( ls => ls.ServiceInfo.ServiceFullName == name );
            Func<string,LivePluginInfo> pluginFinder = id => _plugins.GetByKey( id ) ?? pluginsToAdd.First( lp => lp.PluginInfo.PluginFullName == id );

            using( notifier.SilentMode() )
            {
                foreach( var ls in servicesToAdd ) ls.Bind( solver.FindExistingService( ls.ServiceInfo.ServiceFullName ), serviceFinder, pluginFinder, notifier );
            }
            foreach( var ls in _services ) ls.Bind( solver.FindExistingService( ls.ServiceInfo.ServiceFullName ), serviceFinder, pluginFinder, notifier );

            using( notifier.SilentMode() )
            {
                foreach( var lp in pluginsToAdd ) lp.Bind( solver.FindExistingPlugin( lp.PluginInfo.PluginFullName ), serviceFinder, notifier );
            }
            foreach( var lp in _plugins ) lp.Bind( solver.FindExistingPlugin( lp.PluginInfo.PluginFullName ), serviceFinder, notifier );

            // 4 - It is time to add the new comers: this raises Collection changed "item added" events.
            foreach( var ls in servicesToAdd ) _services.Add( ls );
            foreach( var lp in pluginsToAdd ) _plugins.Add( lp );

            // 5 - Raises all PropertyChanged events for all objects.
            notifier.RaiseEvents();
        }

        internal void UpdateRuntimeErrors( IReadOnlyList<IPluginHostApplyCancellationInfo> errors, Func<string, PluginData> pluginDataFinderForNewPlugin )
        {
            foreach( var e in errors )
            {
                LivePluginInfo pluginInfo = _plugins.GetByKey( e.Plugin.PluginFullName );
                if( pluginInfo == null )
                {
                    pluginInfo = new LivePluginInfo( pluginDataFinderForNewPlugin( e.Plugin.PluginFullName ), _engine );
                    pluginInfo.CurrentError = e;
                    _plugins.Add( pluginInfo );
                }
                else pluginInfo.CurrentError = e;
            }
        }

        /// <summary>
        /// Called by YodiiEngine.StopEngine().
        /// </summary>
        internal void Clear()
        {
            _plugins.Clear();
            _services.Clear();
        }

        /// <summary>
        /// Cancels any start or stop made by this caller.
        /// </summary>
        /// <param name="callerKey">The caller key that identifies the caller. Null is considered to be the same as <see cref="String.Empty"/>.</param>
        /// <returns>Since canceling commands may trigger a runtime error, this method must return a result.</returns>
        public IYodiiEngineResult RevokeCaller( string callerKey = null )
        {
            return _engine.RevokeYodiiCommandCaller( callerKey ?? String.Empty );
        }
    }
}
