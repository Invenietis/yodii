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

        public class Impact
        {
            /// <summary>
            /// The service itself. Never null.
            /// </summary>
            public readonly ServiceProxyBase Service;
            
            /// <summary>
            /// The plugin to disable. Null if Implementation is not null.
            /// </summary>
            public readonly PluginProxy PluginToDisable;
            
            /// <summary>
            /// The service generalization if it exists.
            /// </summary>
            public Impact ServiceGeneralization;
            
            /// <summary>
            /// When SwappedImplementation is not null, this is the plugin that 
            /// is stopping and Starting is true.
            /// When this is null, this is a disabled impact and PluginToDisable is not null.
            /// </summary>
            public readonly StContext Implementation;
            
            /// <summary>
            /// If it is not starting, then it is stopping (and SwappedImplementation 
            /// is necessarily null).
            /// </summary>
            public bool Starting;

            /// <summary>
            /// Plugin that replaces the current Implementation.
            /// </summary>
            public StStartContext SwappedImplementation;

            public Impact( ServiceHost serviceHost, IServiceInfo service, bool starting, StContext impl, PluginProxy pluginToDisable )
            {
                Debug.Assert( service != null );
                Debug.Assert( impl != null || (!starting && p != null), "impl == null => starting is false and a plugin to disable is available." );
                Service = serviceHost.EnsureProxyForDynamicService( service );
                Implementation = impl;
                Starting = starting;
                PluginToDisable = pluginToDisable;
            }
        }

        public ServiceManager( ServiceHost serviceHost )
        {
            _serviceHost = serviceHost;
            _services = new Dictionary<IServiceInfo, Impact>();
        }

        public Impact AddToDisable( IServiceInfo s, PluginProxy p )
        {
            Debug.Assert( p.PluginKey.Service == s );
            return AddToStopOrDisabled( s, null, p );
        }

        public Impact AddToStop( IServiceInfo s, StContext impl )
        {
            return AddToStopOrDisabled( s, impl, null );
        }

        Impact AddToStopOrDisabled( IServiceInfo s, StContext impl, PluginProxy p )
        {
            Impact impact;
            if( _services.TryGetValue( s, out impact ) )
            {
                // Since we register plugins to stop first, if an impact has already been created, 
                // then we have a duplicate plugin in disabled or stoppedPlugins.
                throw new CKException( R.HostApplyInvalidGeneralizationMismatchStopped );
            }
            impact = new Impact( _serviceHost, s, false, impl, p );
            _services.Add( s, impact );
            if( s.Generalization != null )
            {
                impact.ServiceGeneralization = AddToStopOrDisabled( s.Generalization, impl, p );
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
                impact.Starting = true;
                impact.SwappedImplementation = c;
                Debug.Assert( impact.Service != null );
                // If impact is the first to be found, it necessarily corresponds to the most 
                // specialized service in common: sets the service and the stopped plugin.
                if( c.PreviousPluginCommonService == null )
                {
                    c.Status = StContext.StStatus.StartingSwap;
                    c.PreviousPluginCommonService = (IYodiiService)impact.Service;
                    c.PreviousImpl = impact.Implementation;
                    c.PreviousImpl.Status = StContext.StStatus.StoppingSwap;
                }
            }
            else
            {
                impact = new Impact( _serviceHost, s, true, c );
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
