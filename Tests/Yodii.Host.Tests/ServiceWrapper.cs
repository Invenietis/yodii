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
        readonly IYodiiEngine _engine;
        readonly PluginHost _host;
        readonly string _serviceName;
        readonly List<ServiceStatus> _events;

        protected ServiceWrapper( Type t, IYodiiEngine engine, PluginHost host )
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

        public ServiceWrapper CheckState( ServiceStatus s )
        {
            Assert.That( Service.Status, Is.EqualTo( s ), "Service '{0}' status is {1}.", _serviceName, s );
            Assert.That( Live.IsRunning, Is.EqualTo( s.IsStarted() ), "Service '{0}' live is {1}.", _serviceName, Live.IsRunning ? "running" : "not running" );
            return this;
        }
    }

    class ServiceWrapper<T> : ServiceWrapper where T : IYodiiService
    {
        public ServiceWrapper( IYodiiEngine engine, PluginHost host )
            : base( typeof(T), engine, host )
        {
        }

        public new IService<T> Service 
        { 
            get { return (IService<T>)base.Service; } 
        }

    }

}
