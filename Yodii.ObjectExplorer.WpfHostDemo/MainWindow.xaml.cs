using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.WpfHostDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            ObjectExplorerManager m = new ObjectExplorerManager();

            m.Run();

            InitializeComponent();
        }
    }

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
            IAssemblyInfo ia2 = _discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.WpfHostDemo.exe" ) );

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

        object ResolveUnknownType( Type t )
        {
            if( typeof( IYodiiEngine ).IsAssignableFrom( t ) ) return _engine;

            throw new InvalidOperationException( String.Format( "Could not resolve unknown type '{0}'", t.FullName ) );
        }
    }
}
