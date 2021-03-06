#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\ConfigurationEditor\CreateConfigurationItemWindowViewModel.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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
                else return ((PluginInfo)SelectedItem).PluginFullName;
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
