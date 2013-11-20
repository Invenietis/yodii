using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab.ConfigurationEditor
{
    /// <summary>
    /// VM of Configuration editor.
    /// 
    /// Note that this one acts as controller for all ConfigurationManager-related entities, without descending into ConfigurationLayer/ConfigurationItem view models.
    /// </summary>
    class ConfigurationEditorWindowViewModel : ViewModelBase
    {
        #region Fields
        readonly ConfigurationManager _configurationManager;
        readonly ServiceInfoManager _serviceInfoManager;

        readonly ICommand _addLayerCommand;
        readonly ICommand _removeLayerCommand;
        readonly ICommand _addConfigItemCommand;
        readonly ICommand _removeConfigItemCommand;
        readonly ICommand _setConfigItemStatusCommand;
        readonly ICommand _clearOptionalItemsCommand;
        #endregion

        #region Constructor & init
        internal ConfigurationEditorWindowViewModel( ConfigurationManager configManager, ServiceInfoManager serviceManager )
        {
            Debug.Assert( configManager != null );
            Debug.Assert( serviceManager != null );

            _configurationManager = configManager;
            _serviceInfoManager = serviceManager;

            _addLayerCommand = new RelayCommand( ExecuteAddLayer );
            _removeLayerCommand = new RelayCommand( ExecuteRemoveLayer );
            _addConfigItemCommand = new RelayCommand( ExecuteAddConfigItem );
            _removeConfigItemCommand = new RelayCommand( ExecuteRemoveConfigItem );
            _setConfigItemStatusCommand = new RelayCommand( ExecuteSetConfigItemStatus );
            _clearOptionalItemsCommand = new RelayCommand( ExecuteClearOptionalItems );
        }

        #endregion

        #region Properties
        public ConfigurationManager ConfigurationManager { get { return _configurationManager; } }
        public ServiceInfoManager ServiceInfoManager { get { return _serviceInfoManager; } }

        public ICommand AddLayerCommand { get { return _addLayerCommand; } }
        public ICommand RemoveLayerCommand { get { return _removeLayerCommand; } }
        public ICommand AddConfigItemCommand { get { return _addConfigItemCommand; } }
        public ICommand RemoveConfigItemCommand { get { return _removeConfigItemCommand; } }
        public ICommand SetConfigItemStatusCommand { get { return _setConfigItemStatusCommand; } }
        public ICommand ClearOptionalItemsCommand { get { return _clearOptionalItemsCommand; } }
        #endregion

        #region Command handlers
        private void ExecuteClearOptionalItems( object param )
        {
            List<ConfigurationLayer> emptyLayersToDelete = new List<ConfigurationLayer>();

            foreach( ConfigurationLayer layer in _configurationManager.Layers )
            {
                foreach( string serviceOrPluginId in layer.Items.Where( x => x.Status == ConfigurationStatus.Optional ).Select( x => x.ServiceOrPluginId ).ToList() )
                {
                    var itemRemoveResult = layer.Items.Remove( serviceOrPluginId );
                    if( !itemRemoveResult )
                    {
                        MessageBox.Show( "Item remove failure" ); // TODO: Detailed message
                    }
                }

                if( layer.Items.Count == 0 ) emptyLayersToDelete.Add( layer );
            }

            foreach( var layerToDelete in emptyLayersToDelete )
            {
                var layerRemoveResult = _configurationManager.Layers.Remove( layerToDelete );
                if( !layerRemoveResult )
                {
                    MessageBox.Show( "Layer remove failure" ); // TODO: Detailed message
                }
            }
        }

        private void ExecuteSetConfigItemStatus( object param )
        {
            ComboBox box = (ComboBox)param;
            ConfigurationItem item = (ConfigurationItem)box.DataContext;

            ConfigurationStatus newStatus = (ConfigurationStatus)box.SelectedItem;

            var itemSetResult = item.SetStatus( newStatus, "ConfigurationEditor" );
            if( !itemSetResult )
            {
                MessageBox.Show( "Item status set failure" ); // TODO: Detailed message
            }

            box.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateTarget();
        }

        private void ExecuteRemoveConfigItem( object param )
        {
            Debug.Assert( param is ConfigurationItem );
            ConfigurationItem item = (ConfigurationItem)param;

            var itemRemoveResult = item.Layer.Items.Remove( item.ServiceOrPluginId );
            if( !itemRemoveResult )
            {
                MessageBox.Show( "Item remove failure" ); // TODO: Detailed message
            }
        }

        private void ExecuteAddConfigItem( object param )
        {
            Debug.Assert( param != null && param is ConfigurationLayer);

            ConfigurationLayer layer = param as ConfigurationLayer;
            CreateConfigurationItemWindow w = new CreateConfigurationItemWindow(_serviceInfoManager);

            var windowResult = w.ShowDialog();

            if(windowResult == true)
            {
                string serviceOrPluginId = w.ViewModel.SelectedServiceOrPluginId;
                if( serviceOrPluginId != null )
                {
                    var itemAddResult = layer.Items.Add( serviceOrPluginId, w.ViewModel.SelectedStatus );
                    if( !itemAddResult )
                    {
                        MessageBox.Show( "Item add failure" ); // TODO: Detailed message
                    }
                }
            }
            //Debug.Assert( param is Object[] );
            //Object[] objects = (Object[])param;

            //Debug.Assert( objects.Length == 3 );
            //Debug.Assert( objects[0] is ConfigurationLayer );
            //Debug.Assert( objects[1] is ConfigurationStatus );
            //Debug.Assert( objects[2] is PluginInfo || objects[2] is ServiceInfo );

            //throw new NotImplementedException();
        }

        private void ExecuteRemoveLayer( object param )
        {
            Debug.Assert( param is ConfigurationLayer );
            ConfigurationLayer layer = (ConfigurationLayer)param;

            bool cancelled = false;

            if( layer.Items.Count != 0 )
            {
                var messageBoxResult = MessageBox.Show( "Really delete this layer ?\n\nIts configuration will be lost.", "Confirm layer deletion", MessageBoxButton.YesNo, MessageBoxImage.Warning, MessageBoxResult.No, MessageBoxOptions.None );
                if (messageBoxResult != MessageBoxResult.Yes)
                {
                    cancelled = true;
                }
            }
            if( !cancelled )
            {
                var layerRemoveResult = layer.ConfigurationManager.Layers.Remove( layer );

                if( !layerRemoveResult )
                {
                    MessageBox.Show( "Remove failure" ); // TODO: Detailed message
                }
            }
        }

        private void ExecuteAddLayer( object param )
        {
            CreateConfigurationLayerWindow w = new CreateConfigurationLayerWindow();
            bool? result = w.ShowDialog();
            if( result == true )
            {
                string layerName = w.NewLayerName.Text;

                ConfigurationLayer newLayer = new ConfigurationLayer( layerName );
                var layerAddResult = _configurationManager.Layers.Add( newLayer );

                if( !layerAddResult )
                {
                    MessageBox.Show( "Add failure" ); // TODO: Detailed message
                }
            }
        }
        #endregion

        #region Private methods
        #endregion
    }
}
