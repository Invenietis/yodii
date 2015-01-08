#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationSolver\Live\LiveServiceInfo.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    class LiveServiceInfo : LiveYodiiItemInfo, ILiveServiceInfo, INotifyRaisePropertyChanged
    {
        IServiceInfo _serviceInfo;
        ILiveServiceInfo _generalization;
        ILivePluginInfo _runningPlugin;
        ILivePluginInfo _lastRunningPlugin;

        internal LiveServiceInfo( ServiceData s, YodiiEngine engine )
            : base( engine, s, s.ServiceInfo.ServiceFullName )
        {
            _serviceInfo = s.ServiceInfo;
        }

        public override bool IsPlugin { get { return false; } }

        internal void UpdateFrom( ServiceData s, DelayedPropertyNotification notifier )
        {
            UpdateItem( s, notifier );
            notifier.Update( this, ref _serviceInfo, s.ServiceInfo, () => ServiceInfo );
        }

        internal void Bind( ServiceData s, Func<string, LiveServiceInfo> serviceFinder, Func<string, LivePluginInfo> pluginFinder, DelayedPropertyNotification notifier )
        {
            var newGeneralization = s.Generalization != null ? serviceFinder( s.Generalization.ServiceInfo.ServiceFullName ) : null;
            notifier.Update( this, ref _generalization, newGeneralization, () => Generalization );

            var familyRunning = s.Family.DynRunningPlugin;
            Debug.Assert( IsRunning == (familyRunning != null && s.IsGeneralizationOf( familyRunning.Service )) );

            ILivePluginInfo newRunningPlugin = null;
            if( IsRunning )
            {
                newRunningPlugin = pluginFinder( familyRunning.PluginInfo.PluginFullName );
            }
            if( _runningPlugin != null )
            {
                notifier.Update( this, ref _lastRunningPlugin, _runningPlugin, () => LastRunningPlugin );
            }
            notifier.Update( this, ref _runningPlugin, newRunningPlugin, () => RunningPlugin );
        }

        public ILiveServiceInfo Generalization { get { return _generalization; } }

        public ILivePluginInfo RunningPlugin { get { return _runningPlugin; } }

        public ILivePluginInfo LastRunningPlugin { get { return _lastRunningPlugin; } }

        public IServiceInfo ServiceInfo { get { return _serviceInfo; } }

    }
}
