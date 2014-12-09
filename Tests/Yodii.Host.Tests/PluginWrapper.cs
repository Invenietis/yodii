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
        readonly PluginHost _host;
        readonly string _pluginFullName;

        public PluginWrapper( Type t, IYodiiEngine engine, PluginHost host )
        {
            _engine = engine;
            _host = host;
            _pluginFullName = t.FullName;
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

        public IPluginProxy PluginProxy  { get; private set; }

        public object Plugin
        {
            get { return PluginProxy != null ? PluginProxy.RealPluginObject : null; }
        }

        public PluginWrapper CheckState( PluginStatus s )
        {
            Assert.That( PluginProxy.Status, Is.EqualTo( s ), "Plugin '{0}' status is {1}.", _pluginFullName, s );
            Assert.That( Live.IsRunning, Is.EqualTo( s == PluginStatus.Started ), "Plugin '{0}' live is {1}.", _pluginFullName, Live.IsRunning ? "running" : "not running" );
            return this;
        }
    }

    class PluginWrapper<T> : PluginWrapper where T : IYodiiPlugin
    {
        public PluginWrapper( IYodiiEngine engine, PluginHost host )
            : base( typeof(T), engine, host )
        {
        }

        public new T Plugin { get { return (T)base.Plugin; } }
    }

}
