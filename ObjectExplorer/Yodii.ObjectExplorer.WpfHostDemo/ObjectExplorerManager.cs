using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.WpfHostDemo
{
    /// <summary>
    /// The actual ObjectExplorer host. Handles
    /// </summary>
    class ObjectExplorerManager
    {
        readonly YodiiEngine _engine;
        readonly StandardDiscoverer _discoverer;
        readonly PluginHost _host;

        internal ObjectExplorerManager()
        {
            _discoverer = new StandardDiscoverer();
            _host = new PluginHost();
            _engine = new YodiiEngine( _host );

            _host.PluginCreator = CustomPluginCreator;

            ResetConfiguration();
        }

        internal IYodiiEngine Engine { get { return _engine; } }

        /// <summary>
        /// Sets the discovered info. Once done, start the engine with Engine.Start().
        /// </summary>
        internal void SetDiscoveredInfo()
        {
            // Load plugin.service assemblies
            IAssemblyInfo ia = _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.Wpf.dll" ) );
            IAssemblyInfo ia2 = _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.WpfHostDemo.exe" ) );
            IDiscoveredInfo info = _discoverer.GetDiscoveredInfo();
            IYodiiEngineResult discoveredInfoResult = _engine.SetDiscoveredInfo( info );
            Debug.Assert( discoveredInfoResult.Success );
        }

        internal void ResetConfiguration()
        {
            _engine.Configuration.Layers.Clear();
            IConfigurationLayer cl = _engine.Configuration.Layers.Create( "ObjectExplorerManager" );
            cl.Items.Add( "Yodii.ObjectExplorer.Wpf.ObjectExplorerPlugin", ConfigurationStatus.Running );
        }

        /// <summary>
        /// Custom plugin instanciator. Can resolve the IYodiiEngine type to the actual engine.
        /// </summary>
        /// <param name="pluginInfo"></param>
        /// <param name="ctorServiceParameters"></param>
        /// <returns></returns>
        IYodiiPlugin CustomPluginCreator( IPluginInfo pluginInfo, object[] ctorServiceParameters )
        {
            var tPlugin = Assembly.Load( pluginInfo.AssemblyInfo.AssemblyName ).GetType( pluginInfo.PluginFullName, true );
            var ctor = tPlugin.GetConstructors().OrderBy( c => c.GetParameters().Length ).Last();

            ParameterInfo[] parameters = ctor.GetParameters();

            object[] ctorParameters = new object[parameters.Length];
            Debug.Assert( ctorParameters.Length >= ctorServiceParameters.Length );

            for( int i = 0; i < parameters.Length; i++ )
            {
                ParameterInfo p = parameters[i];
                object instance = ctorServiceParameters.Length >= (i + 1) ? ctorServiceParameters[i] : null;

                if( instance != null )
                {
                    ctorParameters[i] = instance;
                }
                else
                {
                    ctorParameters[i] = ResolveUnknownType( p.ParameterType );
                }
            }

            return (IYodiiPlugin)ctor.Invoke( ctorParameters );
        }

        /// <summary>
        /// Custom, hard-coded type resolver. Can resolve the IYodiiEngine type to the actual engine.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        object ResolveUnknownType( Type t )
        {
            if( typeof( IYodiiEngine ).IsAssignableFrom( t ) ) return _engine;

            throw new InvalidOperationException( String.Format( "Could not resolve unknown type '{0}'", t.FullName ) );
        }
    }
}
