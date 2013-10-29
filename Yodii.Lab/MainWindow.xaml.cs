using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Fluent;
using Yodii.Model.CoreModel;
using System.Timers;
using System.Threading;
using Yodii.Model;
using System.Diagnostics;
using Yodii.Lab.ConfigurationEditor;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : RibbonWindow
    {
        readonly MainWindowViewModel _vm;
        ConfigurationEditorWindow _activeConfEditorWindow = null;

        public MainWindow()
        {
            _vm = new MainWindowViewModel();
            this.DataContext = _vm;


            /**
             * Graph display example.
             */
            IServiceInfo serviceA = _vm.CreateNewService( "Service.A" );
            IServiceInfo serviceB = _vm.CreateNewService( "Service.B" );
            IServiceInfo serviceAx = _vm.CreateNewService( "Service.Ax", serviceA );

            IPluginInfo pluginA1 = _vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.A1", serviceA );
            IPluginInfo pluginA2 = _vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.A2", serviceA );
            _vm.SetPluginDependency( pluginA2, serviceB, RunningRequirement.Running );
            IPluginInfo pluginB1 = _vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.B1", serviceB );
            IPluginInfo pluginAx1 = _vm.CreateNewPlugin( Guid.NewGuid(), "Plugin.Ax1", serviceAx );

            /** Manager example. **/
            ConfigurationLayer baseLayer = new ConfigurationLayer("Base layer");
            baseLayer.Items.Add("Service.A", ConfigurationStatus.Running);
            baseLayer.Items.Add("Service.B", ConfigurationStatus.Runnable);
            bool result = _vm.ConfigurationManager.Layers.Add(baseLayer);
            Debug.Assert( result == true );

            ConfigurationLayer secondaryLayer = new ConfigurationLayer();
            secondaryLayer.Items.Add(pluginB1.PluginId.ToString(), ConfigurationStatus.Disable);
            result = _vm.ConfigurationManager.Layers.Add(secondaryLayer);
            Debug.Assert(result == true);

            InitializeComponent();

            // Reorder graph after 2 sec.
            Task.Factory.StartNew(new Action(() =>
            {
                Thread.Sleep(2000);
                Application.Current.Dispatcher.BeginInvoke(new Action(() => { setLayout(); }));
            }));
        }

        private void setLayout()
        {
            this.graphLayout.LayoutAlgorithmType = "ISOM";
        }

        private void ConfEditorButton_Click(object sender, RoutedEventArgs e)
        {
            if( _activeConfEditorWindow != null )
            {
                _activeConfEditorWindow.Activate();
            }
            else
            {
                _activeConfEditorWindow = new ConfigurationEditorWindow(_vm.ConfigurationManager);
                _activeConfEditorWindow.Closing += (s, e2) => { _activeConfEditorWindow = null; };

                _activeConfEditorWindow.Show();
            }
        }
    }
}
