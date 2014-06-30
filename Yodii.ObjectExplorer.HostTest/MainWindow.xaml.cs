using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
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

namespace Yodii.ObjectExplorer.HostTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly YodiiEngine _engine;

        public MainWindow()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();

            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.Wpf.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();

            _engine = new YodiiEngine( host );

            host.PluginCreator = CustomPluginCreator;

            IYodiiEngineResult result1 = _engine.SetDiscoveredInfo( info );

            Debug.Assert( result1.Success );

            IConfigurationLayer cl = _engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.ObjectExplorer.Wpf.ObjectExplorerPlugin", ConfigurationStatus.Running );

            IYodiiEngineResult result = _engine.Start();
            Debug.Assert( result.Success );

            InitializeComponent();
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
            if( typeof( IYodiiEngine ).IsAssignableFrom( t ) )
            {
                return _engine;
            }
            throw new InvalidOperationException( String.Format( "Could not resolve unknown type '{0}'", t.FullName ) );
        }
    }
}
