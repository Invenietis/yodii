using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Linq;
using CK.Core;
using Yodii.Model;
using System.Windows.Input;
using Yodii.Lab.Mocks;
using System.Collections.ObjectModel;

namespace Yodii.Lab.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for ConfigurationEditorWindow.xaml
    /// </summary>
    public partial class ConfigurationEditorWindow : Window, INotifyPropertyChanged
    {
        #region Fields

        /// <summary>
        /// Used when resetting combo boxes without triggering ConfigurationManager Add events.
        /// Prevents reentrancy.
        /// </summary>
        bool isResettingSelection;

        readonly ConfigurationManager _configurationManager;
        readonly ObservableCollection<ConfigurationLayerViewModel> _layerViewModels;
        readonly ServiceInfoManager _serviceInfoManager;
        Visibility _newLayerVisibility = Visibility.Hidden;

        #endregion Fields

        #region Constructor
        internal ConfigurationEditorWindow( ConfigurationManager configurationManager, ServiceInfoManager serviceInfoManager )
        {
            _configurationManager = configurationManager;
            _serviceInfoManager = serviceInfoManager;
            _layerViewModels = new ObservableCollection<ConfigurationLayerViewModel>();
            foreach( var layer in _configurationManager.Layers )
            {
                _layerViewModels.Add( new ConfigurationLayerViewModel( layer, _serviceInfoManager ) );
            }
            _configurationManager.Layers.CollectionChanged += Layers_CollectionChanged;

            this.DataContext = this;

            InitializeComponent();
        }

        void Layers_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ConfigurationLayer addedLayer = (ConfigurationLayer)e.NewItems[0];
                    ConfigurationLayerViewModel vm = new ConfigurationLayerViewModel( addedLayer, ServiceInfoManager );
                    _layerViewModels.Add( vm );
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ConfigurationLayer removedLayer = (ConfigurationLayer)e.OldItems[0];
                    ConfigurationLayerViewModel vmToRemove = _layerViewModels.Where( x => x.Layer == removedLayer ).FirstOrDefault();
                    if( vmToRemove != null )
                        _layerViewModels.Remove( vmToRemove );
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
            }
            RaisePropertyChanged( "EmptyManagerMessageVisibility" );
        }
        #endregion Constructor

        #region Properties

        public ConfigurationManager ConfigurationManager
        {
            get
            {
                return _configurationManager;
            }
        }
        public ServiceInfoManager ServiceInfoManager
        {
            get
            {
                return _serviceInfoManager;
            }
        }

        public Visibility NewLayerVisibility
        {
            get { return _newLayerVisibility; }
            set
            {
                if( value != _newLayerVisibility )
                {
                    _newLayerVisibility = value;
                    RaisePropertyChanged( "NewLayerVisibility" );
                }
            }
        }

        public Visibility EmptyManagerMessageVisibility
        {
            get { return ConfigurationManager.Layers.Count == 0 ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed; }
        }

        public ObservableCollection<ConfigurationLayerViewModel> LayerViewModels
        {
            get { return _layerViewModels; }
        }

        #endregion Properties

        #region Event handlers
        private void ConfigurationStatusValues_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            // Both at 1 if old value was replaced (and not initialized, like when the window starts up)
            if( !isResettingSelection && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1 )
            {
                // Find the selected item, starting from the bottom
                ComboBox box = sender as ComboBox;
                FrameworkElement parentElement = box.Parent as FrameworkElement; // It should be a DockPanel, in our case
                ConfigurationItemViewModel itemVm = parentElement.DataContext as ConfigurationItemViewModel;
                ConfigurationItem item = itemVm.Item;

                ConfigurationStatus oldStatus = item.Status;
                ConfigurationStatus newStatus = (ConfigurationStatus)e.AddedItems[0];

                // Now we can change it.
                var statusChangeResult = item.SetStatus( newStatus, "ConfigurationEditor" );

                if( !statusChangeResult.IsSuccessful )
                {
                    System.Windows.MessageBox.Show(
                        String.Format( "Couldn't change status to {0}:\n{1}", newStatus, String.Join( "; ", statusChangeResult.FailureCauses ) )
                        ,
                        "Status change failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK,
                        MessageBoxOptions.None
                        );
                    isResettingSelection = true;
                    box.SelectedItem = oldStatus;
                }
            }
            isResettingSelection = false;
        }

        private void DeleteConfigurationItemButton_Click( object sender, RoutedEventArgs e )
        {
            // Find the selected item, starting from the bottom
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement; // It should be a DockPanel, in our case
            ConfigurationItem item = parentElement.DataContext as ConfigurationItem;

            var deleteItemResult = item.Layer.Items.Remove( item.ServiceOrPluginId );

            if( !deleteItemResult.IsSuccessful )
            {
                System.Windows.MessageBox.Show(
                    String.Format( "Couldn't remove {0} from layer:\n{1}", item.ServiceOrPluginId, String.Join( "; ", deleteItemResult.FailureCauses ) )
                    ,
                    "Removal failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
        }

        private void NewLayerButton_Click( object sender, RoutedEventArgs e )
        {
            string newLayerName = this.NewLayerNameTextBox.Text;

            ConfigurationLayer newLayer = new ConfigurationLayer( newLayerName );

            var layerAddResult = _configurationManager.Layers.Add( newLayer );

            if( !layerAddResult.IsSuccessful )
            {
                System.Windows.MessageBox.Show(
                    String.Format( "Couldn't add a new layer:\n{0}", String.Join( "; ", layerAddResult.FailureCauses ) )
                    ,
                    "Layer add failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
            else
            {
                this.NewLayerNameTextBox.Clear();
                this.NewLayerVisibility = System.Windows.Visibility.Hidden;
            }
        }

        private void DeleteLayerButton_Click( object sender, RoutedEventArgs e )
        {
            // Find the selected item, starting from the bottom
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement; // StackPanel
            ConfigurationLayerViewModel layerVm = parentElement.DataContext as ConfigurationLayerViewModel;

            var confirmMessageBoxResult = System.Windows.MessageBox.Show( "Delete this layer? Its configuration will be lost.", "Confirm layer deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None );
            if( confirmMessageBoxResult == MessageBoxResult.Yes )
            {
                var deleteLayerResult = _configurationManager.Layers.Remove( layerVm.Layer );

                if( !deleteLayerResult.IsSuccessful )
                {
                    System.Windows.MessageBox.Show(
                        String.Format( "Couldn't remove layer:\n{0}", String.Join( "; ", deleteLayerResult.FailureCauses ) )
                        ,
                        "Layer removal failed",
                        MessageBoxButton.OK,
                        MessageBoxImage.Error,
                        MessageBoxResult.OK,
                        MessageBoxOptions.None
                        );
                }
            }
        }
        #endregion Event handlers

        #region INotifyPropertyChanged utilities

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Fill with Member name, when called from a property.</param>
        protected void RaisePropertyChanged( string caller )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities

        private void ShowNewLayerButton_Click( object sender, RoutedEventArgs e )
        {
            // Toggle visibility
            if( NewLayerVisibility == System.Windows.Visibility.Visible )
            {
                NewLayerVisibility = System.Windows.Visibility.Hidden;
            }
            else
            {
                NewLayerVisibility = System.Windows.Visibility.Visible;
                this.NewLayerNameTextBox.Focus();
            }
        }

        private void ClearOptionalsButton_Click( object sender, RoutedEventArgs e )
        {
            foreach( ConfigurationLayer layer in ConfigurationManager.Layers )
            {
                foreach( ConfigurationItem item in layer.Items.Where( i => i.Status == ConfigurationStatus.Optional ).ToList() )
                {
                    layer.Items.Remove( item.ServiceOrPluginId );
                }
            }
        }

        private void CloseButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        protected override void OnPreviewKeyDown( KeyEventArgs e )
        {
            if( e.Key == Key.Escape ) e.Handled = true;
            base.OnPreviewKeyDown( e );
        }

        private void NewLayerName_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( e.Key == System.Windows.Input.Key.Enter )
            {
                e.Handled = true;
                NewLayerButton_Click( this, new RoutedEventArgs() );
            } else if( e.Key == Key.Escape )
            {
                NewLayerVisibility = System.Windows.Visibility.Hidden;
            }
        }

        private void NewPluginOrServiceId_KeyDown( object sender, KeyEventArgs e )
        {
            if( e.Key == System.Windows.Input.Key.Enter )
            {
                e.Handled = true;
                ConfigurationLayerViewModel vm = (ConfigurationLayerViewModel)((sender as TextBox).Parent as FrameworkElement).DataContext;

                vm.AddItemCommand.Execute( null );
            }
        }

        private void NewPluginOrServiceId_IsVisibleChanged( object sender, DependencyPropertyChangedEventArgs e )
        {
            if( (bool)e.NewValue == true ) (sender as TextBox).Focus();
        }

        private void ServicePluginDefinitionMenuItem_Click( object sender, RoutedEventArgs e )
        {
            ConfigurationLayerViewModel vm = (ConfigurationLayerViewModel)(sender as ContextMenu).DataContext;

            object selection = (e.OriginalSource as MenuItem).DataContext;

            vm.SelectedServiceOrPlugin = selection;
        }
    }

 
}
