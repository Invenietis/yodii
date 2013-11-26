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
        readonly IConfigurationManager _configurationManager;
        readonly ServiceInfoManager _serviceInfoManager;
        readonly ConfigurationEditorWindow _window;

        readonly ICommand _addLayerCommand;
        readonly ICommand _removeLayerCommand;
        readonly ICommand _addConfigItemCommand;
        readonly ICommand _removeConfigItemCommand;
        readonly ICommand _setConfigItemStatusCommand;
        readonly ICommand _clearOptionalItemsCommand;

        bool _isChangingConfig = false; // Prevents reentrancy, and command being run twice. Use when adding/changing items.
        /* When creating a ConfigurationItem that already exists, the existing item's status is changed.
         * In this editor, the existing item's status (in the ComboBox) is changed.
         * That raises its SelectedItemChanged event (used when changing the Status of each item manually),
         * and runs the SetConfigItemStatusCommand, effectively trying to set the ConfigurationStatus to the one it just changed to.
         * Get it? Here's a duck art I found on the net.
         *        ..---.. 
                 .'  _    `. 
             __..'  (o)    : 
            `..__          ; 
                 `.       / 
                   ;      `..---...___ 
                 .'                   `~-. .-') 
                .                         ' _.' 
               :                           : 
               \                           ' 
                +                         J 
                 `._                   _.' 
                    `~--....___...---~' mh 
         * */
        #endregion

        #region Constructor & init
        internal ConfigurationEditorWindowViewModel( ConfigurationEditorWindow parentWindow, IConfigurationManager configManager, ServiceInfoManager serviceManager )
        {
            Debug.Assert( configManager != null );
            Debug.Assert( serviceManager != null );

            _configurationManager = configManager;
            _serviceInfoManager = serviceManager;
            _window = parentWindow;

            _addLayerCommand = new RelayCommand( ExecuteAddLayer );
            _removeLayerCommand = new RelayCommand( ExecuteRemoveLayer );
            _addConfigItemCommand = new RelayCommand( ExecuteAddConfigItem );
            _removeConfigItemCommand = new RelayCommand( ExecuteRemoveConfigItem );
            _setConfigItemStatusCommand = new RelayCommand( ExecuteSetConfigItemStatus );
            _clearOptionalItemsCommand = new RelayCommand( ExecuteClearOptionalItems );
        }

        #endregion

        #region Properties
        public IConfigurationManager ConfigurationManager { get { return _configurationManager; } }
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
            List<IConfigurationLayer> emptyLayersToDelete = new List<IConfigurationLayer>();

            foreach( IConfigurationLayer layer in _configurationManager.Layers )
            {
                foreach( string serviceOrPluginId in layer.Items.Where( x => x.Status == ConfigurationStatus.Optional ).Select( x => x.ServiceOrPluginId ).ToList() )
                {
                    var itemRemoveResult = layer.Items.Remove( serviceOrPluginId );
                    if( !itemRemoveResult.Success )
                    {
                        MessageBox.Show( "Item remove failure" ); // TODO: Detailed message
                    }
                }

                if( layer.Items.Count == 0 ) emptyLayersToDelete.Add( layer );
            }

            foreach( var layerToDelete in emptyLayersToDelete )
            {
                var layerRemoveResult = _configurationManager.Layers.Remove( layerToDelete );
                if( !layerRemoveResult.Success )
                {
                    MessageBox.Show( "Layer remove failure" ); // TODO: Detailed message
                }
            }
        }

        private void ExecuteSetConfigItemStatus( object param )
        {
            if( _isChangingConfig ) return;
            _isChangingConfig = true;
            ComboBox box = (ComboBox)param;
            IConfigurationItem item = (IConfigurationItem)box.DataContext;

            ConfigurationStatus newStatus = (ConfigurationStatus)box.SelectedItem;

            var itemSetResult = item.SetStatus( newStatus, "ConfigurationEditor" );
            if( !itemSetResult.Success )
            {
                MessageBox.Show( "Item status set failure" ); // TODO: Detailed message
            }

            _isChangingConfig = false;
            box.GetBindingExpression(ComboBox.SelectedItemProperty).UpdateTarget();
        }

        private void ExecuteRemoveConfigItem( object param )
        {
            Debug.Assert( param is IConfigurationItem );
            IConfigurationItem item = (IConfigurationItem)param;

            var itemRemoveResult = item.Layer.Items.Remove( item.ServiceOrPluginId );
            if( !itemRemoveResult.Success )
            {
                MessageBox.Show( "Item remove failure" ); // TODO: Detailed message
            }
        }

        private void ExecuteAddConfigItem( object param )
        {
            if( _isChangingConfig ) return;
            _isChangingConfig = true;
            Debug.Assert( param != null && param is IConfigurationLayer);

            IConfigurationLayer layer = param as IConfigurationLayer;
            CreateConfigurationItemWindow w = new CreateConfigurationItemWindow( _serviceInfoManager );
            w.Owner = _window;

            var windowResult = w.ShowDialog();

            if(windowResult == true)
            {
                string serviceOrPluginId = w.ViewModel.SelectedServiceOrPluginId;
                if( serviceOrPluginId != null )
                {
                    var itemAddResult = layer.Items.Add( serviceOrPluginId, w.ViewModel.SelectedStatus );
                    if( !itemAddResult.Success )
                    {
                        MessageBox.Show( "Item add failure" ); // TODO: Detailed message
                    }
                }
            }
            _isChangingConfig = false;
        }

        private void ExecuteRemoveLayer( object param )
        {
            Debug.Assert( param is IConfigurationLayer );
            IConfigurationLayer layer = (IConfigurationLayer)param;

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

                if( !layerRemoveResult.Success )
                {
                    MessageBox.Show( "Remove failure" ); // TODO: Detailed message
                }
            }
        }

        private void ExecuteAddLayer( object param )
        {
            CreateConfigurationLayerWindow w = new CreateConfigurationLayerWindow();
            w.Owner = _window;

            bool? result = w.ShowDialog();
            if( result == true )
            {
                string layerName = w.NewLayerName.Text;

                IConfigurationLayer newLayer = _configurationManager.Layers.Create( layerName );
            }
        }
        #endregion

        #region Private methods
        #endregion
    }
}
