using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ConsoleDemo
{
    class ObjectExplorerManager
    {
        readonly YodiiEngine _engine;
        readonly StandardDiscoverer _discoverer;
        readonly PluginHost _host;

        public ObjectExplorerManager()
        {
            _discoverer = new StandardDiscoverer();
            _host = new PluginHost();
            _engine = new YodiiEngine( _host );

            _host.PluginCreator = CustomPluginCreator;

            IConfigurationLayer cl = _engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.ObjectExplorer.Wpf.ObjectExplorerPlugin", ConfigurationStatus.Running );
        }

        public IYodiiEngine Engine { get { return _engine; } }
        public IYodiiEngineHost EngineHost { get { return _host; } }

        public void Run()
        {
            // Load plugin.service assemblies
            IAssemblyInfo ia = _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.Wpf.dll" ) );
            IAssemblyInfo ia2 = _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.ConsoleDemo.exe" ) );

            IDiscoveredInfo info = _discoverer.GetDiscoveredInfo();

            IYodiiEngineResult discoveredInfoResult = _engine.SetDiscoveredInfo( info );
            Debug.Assert( discoveredInfoResult.Success );

            // Run engine
            IYodiiEngineResult result = _engine.Start();
            Debug.Assert( result.Success );
        }

        IYodiiPlugin CustomPluginCreator( IPluginInfo pluginInfo, object[] ctorServiceParameters )
        {
            var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );
            var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).Last();

            ParameterInfo[] parameters = ctor.GetParameters();

            object[] ctorParameters = new object[parameters.Length];

            int j = 0; // Index for Service parameters
            for( int i = 0; i < parameters.Length; i++ )
            {
                ParameterInfo p = parameters[i];
                if( typeof( IServiceInfo ).IsAssignableFrom( p.ParameterType ) )
                {
                    // For Service parameters, use the given Service parameters array
                    ctorParameters[i] = ctorServiceParameters[j];
                    j++;
                }
                else
                {
                    // Use the resolver (not null here) to try and get missing types
                    ctorParameters[i] = ResolveUnknownType( p.ParameterType );
                }
            }

            return (IYodiiPlugin)ctor.Invoke( ctorParameters );
        }

        object ResolveUnknownType( Type t )
        {
            if( typeof( IYodiiEngine ).IsAssignableFrom( t ) ) return _engine;

            throw new InvalidOperationException( String.Format( "Could not resolve unknown type '{0}'", t.FullName ) );
        }
    }
}
