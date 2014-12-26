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
