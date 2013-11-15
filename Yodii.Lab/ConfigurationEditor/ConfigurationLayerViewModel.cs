using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using System.Linq;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab.ConfigurationEditor
{
    public class ConfigurationLayerViewModel : ViewModelBase
    {
        #region Fields
        readonly ConfigurationLayer _layer;
        readonly ConfigurationEditorWindow _window;
        readonly ObservableCollection<ConfigurationItemViewModel> _itemViewModels;

        Visibility _newLayerVisibility = Visibility.Collapsed;
        object _selectedServiceOrPlugin;
        ConfigurationStatus _newItemStatus;
        #endregion Fields

        #region Properties
        public ICommand AddItemCommand { get; private set; }
        public ICommand ToggleNewItemCommand { get; private set; }

        public ConfigurationLayer Layer
        {
            get { return _layer; }
        }

        public ServiceInfoManager ServiceInfoManager
        {
            get;
            private set;
        }

        public ObservableCollection<ConfigurationItemViewModel> ItemViewModels
        {
            get { return _itemViewModels; }
        }

        public Visibility NewLayerVisibility
        {
            get
            {
                return _newLayerVisibility;
            }
            set
            {
                if( value != _newLayerVisibility )
                {
                    _newLayerVisibility = value;
                    RaisePropertyChanged( "NewLayerVisibility" );
                }
            }
        }

        public ConfigurationStatus NewConfigurationStatus
        {
            get
            {
                return _newItemStatus;
            }
            set
            {
                if( value != _newItemStatus )
                {
                    _newItemStatus = value;
                    RaisePropertyChanged( "NewConfigurationStatus" );
                }
            }
        }

        public string NewItemDescription
        {
            get
            {
                if( SelectedServiceOrPlugin is ServiceInfo )
                {
                    return String.Format( "Service: {0}", ((ServiceInfo)SelectedServiceOrPlugin).ServiceFullName );
                }
                else if( SelectedServiceOrPlugin is PluginInfo )
                {
                    return String.Format( "Plugin: {0}", ((PluginInfo)SelectedServiceOrPlugin).Description );
                }
                return String.Empty;
            }
        }

        public object SelectedServiceOrPlugin
        {
            get
            {
                return _selectedServiceOrPlugin;
            }
            set
            {
                Debug.Assert( value == null || value is ServiceInfo || value is PluginInfo );
                if( value != _selectedServiceOrPlugin )
                {
                    _selectedServiceOrPlugin = value;
                    RaisePropertyChanged( "SelectedServiceOrPlugin" );
                    RaisePropertyChanged( "NewItemDescription" );
                }
            }
        }
        #endregion Properties

        #region Constructor
        internal ConfigurationLayerViewModel( ConfigurationEditorWindow window, ConfigurationLayer layer, ServiceInfoManager serviceInfoManager )
        {
            Debug.Assert( window != null );
            Debug.Assert( layer != null );

            AddItemCommand = new RelayCommand( ExecuteAddItem );
            ToggleNewItemCommand = new RelayCommand( ExecuteToggleNewItem );

            ServiceInfoManager = serviceInfoManager;
            _window = window;
            _itemViewModels = new ObservableCollection<ConfigurationItemViewModel>();

            _layer = layer;
            foreach( var item in _layer.Items )
            {
                _itemViewModels.Add( new ConfigurationItemViewModel( item, ServiceInfoManager ) );
            }
            _layer.Items.CollectionChanged += Items_CollectionChanged;
        }

        void Items_CollectionChanged( object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e )
        {
            switch( e.Action )
            {
                case System.Collections.Specialized.NotifyCollectionChangedAction.Add:
                    ConfigurationItem addedItem = (ConfigurationItem)e.NewItems[0];
                    ConfigurationItemViewModel vm = new ConfigurationItemViewModel( addedItem, ServiceInfoManager );
                    _itemViewModels.Add( vm );
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Remove:
                    ConfigurationItem removedItem = (ConfigurationItem)e.OldItems[0];
                    ConfigurationItemViewModel vmToRemove = _itemViewModels.Where( x => x.Item == removedItem ).FirstOrDefault();
                    if( vmToRemove != null )
                        _itemViewModels.Remove( vmToRemove );
                    break;
                case System.Collections.Specialized.NotifyCollectionChangedAction.Move:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Replace:
                    throw new NotImplementedException();
                case System.Collections.Specialized.NotifyCollectionChangedAction.Reset:
                    throw new NotImplementedException();
            }
        }
        #endregion Constructor

        #region Private methods
        private void ExecuteToggleNewItem( object obj )
        {
            if( NewLayerVisibility == Visibility.Visible )
            {
                NewLayerVisibility = Visibility.Collapsed;
            }
            else
            {
                NewLayerVisibility = Visibility.Visible;
            }
        }

        private void ExecuteAddItem( object obj )
        {
            if( SelectedServiceOrPlugin == null ) return;
            string serviceOrPluginId = SelectedServiceOrPlugin is ServiceInfo ? ((ServiceInfo)SelectedServiceOrPlugin).ServiceFullName : ((PluginInfo)SelectedServiceOrPlugin).PluginId.ToString();

            _window.IsResettingSelection = true;

            var result = _layer.Items.Add( serviceOrPluginId, _newItemStatus, "ConfigurationEditor" );

            if( !result.IsSuccessful )
            {
                System.Windows.MessageBox.Show(
                    String.Format( "Couldn't add new item to layer:\n{0}", String.Join( "; ", result.FailureCauses ) )
                    ,
                    "Add failed",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error,
                    MessageBoxResult.OK,
                    MessageBoxOptions.None
                    );
            }
            else
            {
                SelectedServiceOrPlugin = null;
                NewConfigurationStatus = ConfigurationStatus.Optional;
                NewLayerVisibility = Visibility.Collapsed;
            }

            _window.IsResettingSelection = false;
        }
        #endregion Private methods
    }
}
