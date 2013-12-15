using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Engine
{
    enum ConfigurationSolverStep
    {
        RegisterServices,
        RegisterPlugins,
        OnAllPluginsAdded,
        PropagatePluginStatus,
        InitializeFinalStartableStatus,
        BlockingDetection,
        DynamicResolution,
        StaticError,
        WaitingForDynamicResolution
    }

    interface IConfigurationSolver
    {
        ConfigurationSolverStep Step { get; }

        ServiceData FindExistingService( string serviceFullName );
        
        PluginData FindExistingPlugin( Guid pluginId );
        
        ServiceData FindService( string serviceFullName );
        
        PluginData FindPlugin( Guid pluginId );
        
        IEnumerable<ServiceData> AllServices { get; }
        
        IEnumerable<PluginData> AllPlugins { get; }
        
        void DeferPropagation( ServiceData s );
    }

}
