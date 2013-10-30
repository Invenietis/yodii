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
using Yodii.Lab.Mocks;

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

            InitializeComponent();

            // Reorder graph after 2 sec.
            // TODO
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

        private void NewGraphLayoutButton_Click(object sender, RoutedEventArgs e)
        {
            MenuItem item = e.OriginalSource as MenuItem;

            String newSelection = item.DataContext as String;

            this.graphLayout.LayoutAlgorithmType = newSelection;
        }

        private void ReorderGraphButton_Click (object sender, RoutedEventArgs e)
        {
            string oldLayout = this.graphLayout.LayoutAlgorithmType;

            this.graphLayout.LayoutAlgorithmType = null;

            this.graphLayout.LayoutAlgorithmType = oldLayout;
        }

        private void StackPanel_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            FrameworkElement vertexPanel = sender as FrameworkElement;

            YodiiGraphVertex vertex = vertexPanel.DataContext as YodiiGraphVertex;

            _vm.SelectVertex(vertex);
        }

        private void graphLayout_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            _vm.SelectVertex(null);
        }
    }
}
