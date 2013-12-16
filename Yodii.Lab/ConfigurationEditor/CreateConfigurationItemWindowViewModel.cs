using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab.ConfigurationEditor
{
    class CreateConfigurationItemWindowViewModel : ViewModelBase
    {
        #region Fields
        readonly LabStateManager _serviceInfoManager;
        readonly ICommand _selectItemCommand;

        object _selectedItem;
        ConfigurationStatus _selectedStatus;
        #endregion

        #region Constructor
        public CreateConfigurationItemWindowViewModel( LabStateManager serviceManager )
        {
            Debug.Assert( serviceManager != null );

            SelectedItem = null;
            SelectedStatus = ConfigurationStatus.Optional;
            _serviceInfoManager = serviceManager;

            _selectItemCommand = new RelayCommand( SelectItemExecute );
        }
        #endregion

        #region Observable properties
        public ConfigurationStatus SelectedStatus
        {
            get
            {
                return _selectedStatus;
            }
            set
            {
                if( value != _selectedStatus )
                {
                    _selectedStatus = value;
                    RaisePropertyChanged();
                }
            }
        }

        public object SelectedItem
        {
            get
            {
                return _selectedItem;
            }
            set
            {
                if( value != _selectedItem )
                {
                    _selectedItem = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "SelectedItemDescription" );
                }
            }
        }

        public string SelectedItemDescription
        {
            get
            {
                if( SelectedItem == null )
                {
                    if( ServiceInfoManager.ServiceInfos.Count == 0 && ServiceInfoManager.PluginInfos.Count == 0 )
                    {
                        return "No items. Create a plugin or service first.";
                    }
                    else
                    {
                        return "Select an existing service or plugin.";
                    }
                }
                else
                {
                    return LabStateManager.GetDescriptionOfServiceOrPluginInfo( SelectedItem );
                }
            }
        }

        #endregion
        #region Properties
        public LabStateManager ServiceInfoManager { get { return _serviceInfoManager; } }
        public ICommand SelectItemCommand { get { return _selectItemCommand; } }

        public string SelectedServiceOrPluginId
        {
            get
            {
                if( SelectedItem == null ) return null;
                else if( SelectedItem is ServiceInfo ) return ((ServiceInfo)SelectedItem).ServiceFullName;
                else return ((PluginInfo)SelectedItem).PluginId.ToString();
            }
        }
        #endregion

        private void SelectItemExecute( object param )
        {
            Debug.Assert( param == null || param is PluginInfo || param is ServiceInfo, "Parameter is ServiceInfo, PluginInfo, or null" );

            SelectedItem = param;
        }
    }
}
