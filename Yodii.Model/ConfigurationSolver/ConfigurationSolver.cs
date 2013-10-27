using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using System.Diagnostics;
using Yodii.Model.CoreModel;

namespace Yodii.Model.ConfigurationSolver
{

    public class ConfigurationSolver
    {
        Dictionary<IServiceInfo,ServiceData> _services;
        List<ServiceRootData> _serviceRoots;
        Dictionary<IPluginInfo,PluginData> _plugins;

        public ConfigurationSolver()
        {
            //TO DO: Save current state into a ConfigState object (state design pattern)
            _services = new Dictionary<IServiceInfo, ServiceData>();
            _serviceRoots = new List<ServiceRootData>();
            _plugins = new Dictionary<IPluginInfo, PluginData>();
        }

        public IConfigurationSolverResult Initialize( FinalConfiguration finalConfig, IEnumerable<IServiceInfo> services, IEnumerable<IPluginInfo> plugins )
        {
            // Registering all Services.
            _services.Clear();
            _serviceRoots.Clear();
            
            foreach( IServiceInfo sI in services )
            {
                // This creates services and applies solved configuration to them: directly disabled services
                // and specializations disabled by their generalizations' configuration are handled.
                RegisterService( finalConfig, sI );
            }
            // Service trees have been built and we have the roots.
            // We can now handle MustRun services: there must be at most one such service by service 
            // root otherwise it is a configuration error.
            foreach( var root in _serviceRoots )
            {
                if( !root.Disabled ) root.InitializeMustExistService();
            }
            // We can now instantiate plugin data. 
            _plugins.Clear();
            foreach( IPluginInfo p in plugins )
            {
                RegisterPlugin( finalConfig, p );
            }
            // Initialize services disabled state based on their available plugins:
            // roots without any available plugins are de facto disabled.
            foreach( var root in _serviceRoots )
            {
                if( !root.Disabled ) root.OnAllPluginsAdded();
            }
            // Now, we apply ServiceReference MustExist constraints from every plugins to their referenced services.
            foreach( PluginData p in _plugins.Values )
            {
                // When a plugin is disabled because of a disabled required service reference and it implements a service, the service
                // becomes disabled (if it has no more available implementations) and that triggers disabling of plugins that require
                // the service. This works because disable flag on each participant is carefully set before propagating the
                // information to others to avoid loops and because such plugins reference themselves at the required service (AddMustExistReferencer).
                if( !p.Disabled && p.ConfigSolvedStatus >= RunningRequirement.Runnable )
                {
                    p.CheckReferencesWhenMustExist();
                }
            }
            List<IPluginInfo> blockingPlugins = null;
            List<IServiceInfo> blockingServices = null;
            
            // Time to conclude about configuration and to initialize dynamic resolution.
            // Any Plugin that has a ConfigSolvedStatus greater or equal to Runnable and is Disabled leads to an impossible configuration.
            foreach( PluginData p in _plugins.Values )
            {
                if( p.Disabled )
                {
                    if( p.ConfigOriginalStatus != ConfigurationStatus.Disable && p.ConfigSolvedStatus >= RunningRequirement.Runnable )
                    {
                        if( blockingPlugins == null ) blockingPlugins = new List<IPluginInfo>();
                        blockingPlugins.Add( p.PluginInfo );
                    }
                }
            }
            // Any Service that has a ConfigSolvedStatus greater or equal to Runnable and is Disabled leads to an impossible configuration.
            foreach( ServiceData s in _services.Values )
            {
                if( s.Disabled )
                {
                    if ( s.ConfigOriginalStatus != ConfigurationStatus.Disable && s.ConfigSolvedStatus >= RunningRequirement.Runnable )
                    {
                        if( blockingServices == null ) blockingServices = new List<IServiceInfo>();
                        blockingServices.Add( s.ServiceInfo );
                    }
                }
            }
            if( blockingPlugins != null || blockingServices != null )
            {
                return new ConfigurationSolverResult( blockingPlugins, blockingServices );
            }
            // Plugin state first: Service plugins state will be updated while 
            // initializing services below.

            // Configurations have been applied: now, PluginData and ServiceData objects have been "configured" as much as possible by following
            // Disabled, MustExist and MustExistAndRun configurations and references (MustExist and MustExistAndRun).
            // As soon as configuration changes, this very first phasis must be done again. But as long as the configuration does NOT change, we can handle
            // dynamic starts and stops based on this very same starting point.
            //
            // Plugins that have not been explicitely disabled nor started (MustExistAndRun) are in an "unknown state".
            // Services that have not been explicitely disabled nor selected (thanks to a MustExistAndRun plugin by example) are also in an "unknown state".
            //
            // This is where the "dynamic commands" must be applied, from the most recent on to the oldest one.
            //
            // Applying a "start command" to a Plugin means starting MustExistServices and restrict the system to MustExist services accordingly and setting the plugin Status to RunningStatus.Running. 
            // This puts some other services and/or plugins into RunningStatus.Stopped state. (Mechanism here is the same as the "static" propagation of configurations.)
            //
            // Applying a "stop command" simply sets the plugin or service Status to RunningStatus.Stopped (and may stop other plugins/services just like "static" propagation of configurations does). 
            // 
            // The first "command" (the most recent one) SHOULD always be satisfied: since it must have been registered (accepted) only for plugin or service with a Status
            // not equals to RunningStatus.RunningLocked or RunningStatus.Disabled (these 2 status are driven by the configuration), the service or plugin is guaranteed to be runnable... or stoppable
            // Of course, this is true only if we are not being processing a Configuration change. 
            // In such case, the first "command" is not necessarily satisfied (and it is handled like the older ones, see below).
            //
            // For remaining "commands", this is not true anymore. Here, the rule is simple: if a start or stop can not be satisfied, the corresponding "command" is removed from the list.
            // Note that if the command is already satisfied (ie. the state is no more "unknown" and is ok), we let the command in the list.
            //
            // When all "commands" have been applied (either successfully or not), there may be services or plugins that are still in "unknown state". For each of them
            // the TryStart (from MustExistTryStart and OptionalTryStart) from the configuration are taken into account.
            // There is 2 ways to apply TryStart: 
            //   1) - we apply them in order (actually transforming TryStart into "start commands"), but the order is meaningless: the result is not deterministic.
            //   2) - we dynamically compute a "best solution" that maximizes TryStart satisfaction.
            // Practically, my (current) feeling is that 2) should be the way to go at the start of the system, but 1) will be enough as soon as we have some (enough) "commands" to honor.
            //
            // What is a "start command" ? How are they managed ?
            // - A command is nothing more than bool (start or stop) associated to a PluginId xor a ServiceFullName.
            // - They are stored in a list, new one added at the head.
            // - They are created by calls to ILivePluginInfo.Start( object caller ) or ILiveServiceInfo.Start( object caller ) methods.
            //    - ILivePluginInfo/ILiveServiceInfo are two observable objects that expose the live status of plugins/services. They are accessible from a ILiveConfiguration object.
            //    - These 2 methods can only be called if ILiveXXXInfo.Status is equal to RunningStatus.Running or RunningStatus.Stopped. 
            //      Calling them when Status is RunningStatus.Disabled or RunningStatus.RunningLocked throws an InvalidOperationException dans ta gueule.
            //    - A given caller can have at most one command for a plugin or a service: Start and Stop push the existing command (with the right boolean) or create a new one at the head of the list.
            //    - Calling Start (resp. Stop) with the same caller twice (without having called Stop - resp. Start) is not an error, it simply "pushes" the command at the head of the list.
            //
            // Caller identifier can be of any object type. They can be revoked thanks to ILiveConfiguration.RevokeCaller( object caller ) that removes from the list all commands emitted by this caller.
            // Nevertheless, one (very) special case of callers must be handled carefully: the Plugins.
            //
            // When a Plugin calls Start() or Stop() on services (or, more rarely, on plugins), it participates to the plan. When the plugin is Stopped, it must be revoked as a caller (it has nothing to
            // say anymore). 
            // The funny case is when the application of a "command" stops a running plugin that emitted commands that have already been applied.
            //
            // This pathological case must be detected (not that complicated) and handled: the simplest way (actually the only one I imagine) to handle this is to revoke the plugin as a caller (this 
            // removes all its commands) and to restart computation from the initial state (from the configuration) by applying all existing commands.
            // Failure to handle this at this level will work... but will result in (potentially multiple) loops that may be visible to the user: the plugin will be revoked when Stopped by 
            // the PluginHost and that will trigger recomputation of the plan.
            // Since we are talking about pathological cases... could there be loops in the System? It has to be investigated but my feeling is:
            //   - Yes, but very very rarely in practice. 
            //   - This can be easily (not too much complicated at least) detected and gracefully signaled by an error.
            // But this is definitely an aspect to investigate.
            // 
            // Once the configuration is computed, it is transferred to the PluginHost. On success, ILiveConfiguration objects are updated. If the PluginHost fails (on exceptions, it does its best 
            // to restore the system), the ILiveConfiguration is not updated with the new plan.
            // 

            //
            List<IPluginInfo> disabledPlugins = new List<IPluginInfo>();
            List<IPluginInfo> runningPlugins = new List<IPluginInfo>();
            List<IPluginInfo> stoppedPlugins = new List<IPluginInfo>();

            // (Temporary) brute force to find a valid configuration.

            return new ConfigurationSolverResult( disabledPlugins, stoppedPlugins, runningPlugins );
        }

        private void CollectResult( List<IPluginInfo> disabledPlugins, List<IPluginInfo> runningPlugins, List<IPluginInfo> stoppedPlugins )
        {
            disabledPlugins.Clear();
            runningPlugins.Clear();
            stoppedPlugins.Clear();
            foreach( PluginData p in _plugins.Values )
            {
                if( p.Disabled ) disabledPlugins.Add( p.PluginInfo );
                //Status comes from PluginData.Dynamic
                else if( p.Status >= RunningStatus.Running ) runningPlugins.Add( p.PluginInfo );
                else stoppedPlugins.Add( p.PluginInfo );
            }
        }

        ServiceData RegisterService( FinalConfiguration finalConfig, IServiceInfo s )
        {
            ServiceData data;
            if( _services.TryGetValue( s, out data ) ) return data;

            //Set default status
            ConfigurationStatus serviceStatus = ConfigurationStatus.Optional;

            serviceStatus = finalConfig.GetStatus(s.ServiceFullName);
            // Handle generalization.
            ServiceData dataGen = null;
            if( s.Generalization != null )
            {
                dataGen = RegisterService( finalConfig, s.Generalization );
            }
            Debug.Assert( (s.Generalization == null) == (dataGen == null) );
            if( dataGen == null )
            {
                var dataRoot = new ServiceRootData( _services, s, serviceStatus, externalService => true );
                _serviceRoots.Add( dataRoot );
                data = dataRoot;
            }
            else
            {
                data = new ServiceData( _services, s, dataGen, serviceStatus, externalService => true );
            }
            _services.Add( s, data );
            return data;
        }

        PluginData RegisterPlugin( FinalConfiguration finalConfig, IPluginInfo p )
        {
            PluginData data;
            if( _plugins.TryGetValue( p, out data ) ) return data;

            //Set default status
            ConfigurationStatus pluginStatus = ConfigurationStatus.Optional;
            pluginStatus = finalConfig.GetStatus( p.PluginId.ToString() );
            ServiceData service = p.Service != null ? _services[ p.Service ] : null;
            data = new PluginData( _services, p, service, pluginStatus );
            _plugins.Add( p, data );
            return data;
        }
    }
}