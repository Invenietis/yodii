using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
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
using Yodii.ObjectExplorer.Wpf;

namespace Yodii.ObjectExplorer.HostTest
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();

            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.ObjectExplorer.Wpf.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            PluginHost host = new PluginHost();
            YodiiEngine engine = new YodiiEngine( host );

            host.Resolver = ( Type t ) =>
            {
                if( typeof( IYodiiEngine ).IsAssignableFrom( t ) )
                {
                    return engine;
                }
                throw new InvalidOperationException( String.Format( "Could not resovle unknown type '{0}'", t.FullName ) );
            };

            IYodiiEngineResult result1 = engine.SetDiscoveredInfo( info );

            Debug.Assert( result1.Success );

            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.ObjectExplorer.Wpf.ObjectExplorerPlugin", ConfigurationStatus.Running );

            IYodiiEngineResult result = engine.Start();
            Debug.Assert( result.Success );

            InitializeComponent();
        }
    }
}
