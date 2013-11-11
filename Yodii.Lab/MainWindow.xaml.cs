using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Timers;
using System.Threading;
using Yodii.Model;
using System.Diagnostics;
using Yodii.Lab.ConfigurationEditor;
using Yodii.Lab.Mocks;
using Yodii.Lab.Utils;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Fluent.RibbonWindow
    {
        readonly MainWindowViewModel _vm;
        ConfigurationEditorWindow _activeConfEditorWindow = null;

        public MainWindow()
        {
            _vm = new MainWindowViewModel();
            this.DataContext = _vm;

            _vm.PropertyChanged += _vm_PropertyChanged;

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
        }

        void _vm_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( e.PropertyName == "ConfigurationManager" && _activeConfEditorWindow != null )
            {
                _activeConfEditorWindow.Close();
            }
        }

        private void ConfEditorButton_Click( object sender, RoutedEventArgs e )
        {
            if( _activeConfEditorWindow != null )
            {
                _activeConfEditorWindow.Activate();
            }
            else
            {
                _activeConfEditorWindow = new ConfigurationEditorWindow( _vm.ConfigurationManager );
                _activeConfEditorWindow.Closing += ( s, e2 ) => { _activeConfEditorWindow = null; };

                _activeConfEditorWindow.Show();
            }
        }

        private void NewGraphLayoutButton_Click( object sender, RoutedEventArgs e )
        {
            MenuItem item = e.OriginalSource as MenuItem;

            String newSelection = item.DataContext as String;

            this.graphLayout.LayoutAlgorithmType = newSelection;
        }

        private void ReorderGraphButton_Click( object sender, RoutedEventArgs e )
        {
            string oldLayout = this.graphLayout.LayoutAlgorithmType;

            this.graphLayout.LayoutAlgorithmType = null;

            this.graphLayout.LayoutAlgorithmType = oldLayout;
        }

        private void StackPanel_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            FrameworkElement vertexPanel = sender as FrameworkElement;

            YodiiGraphVertex vertex = vertexPanel.DataContext as YodiiGraphVertex;

            _vm.SelectedVertex = vertex;
        }

        private void graphLayout_MouseDown( object sender, System.Windows.Input.MouseButtonEventArgs e )
        {
            _vm.SelectedVertex = null;
        }

        private void NewServiceButton_Click( object sender, RoutedEventArgs e )
        {
            IServiceInfo selectedService = null;

            if( _vm.SelectedVertex != null )
            {
                if( _vm.SelectedVertex.IsService )
                {
                    selectedService = _vm.SelectedVertex.LiveServiceInfo.ServiceInfo;
                }
                else if( _vm.SelectedVertex.IsPlugin )
                {
                    selectedService = _vm.SelectedVertex.LivePluginInfo.PluginInfo.Service;
                }
            }

            AddServiceWindow window = new AddServiceWindow( _vm.ServiceInfos, selectedService );

            window.NewServiceCreated += ( s, nse ) =>
            {
                if( _vm.ServiceInfos.Any( si => si.ServiceFullName == nse.ServiceName ) )
                {
                    nse.CancelReason = String.Format( "Service with name {0} already exists. Pick another name.", nse.ServiceName );
                }
                else
                {
                    IServiceInfo newService = _vm.CreateNewService( nse.ServiceName, nse.Generalization );
                    _vm.SelectService( newService );
                }
            };

            window.Owner = this;

            window.ShowDialog();
        }

        private void NewPluginButton_Click( object sender, RoutedEventArgs e )
        {
            IServiceInfo selectedService = null;

            if( _vm.SelectedVertex != null )
            {
                if( _vm.SelectedVertex.IsService )
                {
                    selectedService = _vm.SelectedVertex.LiveServiceInfo.ServiceInfo;
                }
                else if( _vm.SelectedVertex.IsPlugin )
                {
                    selectedService = _vm.SelectedVertex.LivePluginInfo.PluginInfo.Service;
                }
            }

            AddPluginWindow window = new AddPluginWindow( _vm.ServiceInfos, selectedService );

            window.NewPluginCreated += ( s, npe ) =>
            {
                if( _vm.PluginInfos.Any( si => si.PluginId == npe.PluginId ) )
                {
                    npe.CancelReason = String.Format( "Plugin with GUID {0} already exists. Pick another GUID.", npe.PluginId.ToString() );
                }
                else
                {
                    IPluginInfo newPlugin = _vm.CreateNewPlugin( npe.PluginId, npe.PluginName, npe.Service );
                    foreach( var kvp in npe.ServiceReferences )
                    {
                        _vm.SetPluginDependency( newPlugin, kvp.Key, kvp.Value );
                    }
                    _vm.SelectPlugin( newPlugin );
                }
            };

            window.Owner = this;

            window.ShowDialog();
        }

        private void ServiceNamePropertyTextBox_LostFocus( object sender, RoutedEventArgs e )
        {
            UpdateServicePropertyNameWithTextbox( sender as System.Windows.Controls.TextBox );
        }

        private void ServiceNamePropertyTextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( e.Key == System.Windows.Input.Key.Enter )
            {
                UpdateServicePropertyNameWithTextbox( sender as System.Windows.Controls.TextBox );
            }
        }

        private void UpdateServicePropertyNameWithTextbox( System.Windows.Controls.TextBox textBox )
        {
            LiveServiceInfo liveService = textBox.DataContext as LiveServiceInfo;

            if( liveService.ServiceInfo.ServiceFullName == textBox.Text ) return;
            if( String.IsNullOrWhiteSpace( textBox.Text ) ) return;

            DetailedOperationResult r = _vm.RenameService( liveService.ServiceInfo, textBox.Text );

            if( !r )
            {
                textBox.Text = liveService.ServiceInfo.ServiceFullName;
                MessageBox.Show( String.Format( "Couldn't change service name.\n{0}", r.Reason ), "Couldn't change service name", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK );
            }
        }

        private void OpenFile_Click( object sender, RoutedEventArgs e )
        {
            Microsoft.Win32.OpenFileDialog dlg = new Microsoft.Win32.OpenFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "Yodii.Lab XML Files (*.xml)|*.xml";
            dlg.CheckFileExists = true;
            dlg.CheckPathExists = true;

            Nullable<bool> result = dlg.ShowDialog();

            if( result == true )
            {
                string filePath = dlg.FileName;
                var r = _vm.LoadState( filePath );
                if( !r )
                {
                    MessageBox.Show( r.Reason, "Couldn't open file" );
                }
            }
        }

        private void SaveAsFile_Click( object sender, RoutedEventArgs e )
        {
            Microsoft.Win32.SaveFileDialog dlg = new Microsoft.Win32.SaveFileDialog();

            dlg.DefaultExt = ".xml";
            dlg.Filter = "Yodii.Lab XML Files (*.xml)|*.xml";
            dlg.CheckPathExists = true;
            dlg.OverwritePrompt = true;
            dlg.AddExtension = true;

            Nullable<bool> result = dlg.ShowDialog();

            if( result == true )
            {
                string filePath = dlg.FileName;
                var r = _vm.SaveState( filePath );
                if( !r )
                {
                    MessageBox.Show( r.Reason, "Couldn't save file" );
                }
            }
        }

        private void DeleteReferenceButton_Click( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement;
            MockServiceReferenceInfo serviceRef = parentElement.DataContext as MockServiceReferenceInfo;

            serviceRef.Owner.InternalServiceReferences.Remove( serviceRef );
        }

        private bool isResettingSelection;
        private void ReferenceRequirementComboBox_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
        {
            // Both at 1 if old value was replaced (and not initialized, like when the window starts up)
            if( !isResettingSelection && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1 )
            {
                // Find the selected item, starting from the bottom
                ComboBox box = sender as ComboBox;
                FrameworkElement parentElement = box.Parent as FrameworkElement;
                MockServiceReferenceInfo serviceRef = parentElement.DataContext as MockServiceReferenceInfo;

                RunningRequirement oldReq = serviceRef.Requirement;
                RunningRequirement newReq = (RunningRequirement)e.AddedItems[0];

                serviceRef.Requirement = newReq;
            }
            isResettingSelection = false;
        }

        private void CreateReferenceButton_Click( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement;
            LivePluginInfo livePlugin = parentElement.DataContext as LivePluginInfo;

            ComboBox requirementComboBox = parentElement.FindName( "NewReferenceRequirementComboBox" ) as ComboBox;
            ComboBox serviceComboBox = parentElement.FindName( "NewReferenceServiceComboBox" ) as ComboBox;

            ServiceInfo service = serviceComboBox.SelectedItem as ServiceInfo;
            RunningRequirement req = (RunningRequirement)requirementComboBox.SelectedItem;

            livePlugin.PluginInfo.InternalServiceReferences.Add( new MockServiceReferenceInfo( livePlugin.PluginInfo, service, req ) );
        }


    }
}
