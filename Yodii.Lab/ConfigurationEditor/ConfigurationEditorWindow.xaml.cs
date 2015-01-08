#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\ConfigurationEditor\ConfigurationEditorWindow.xaml.cs) is part of CiviKey. 
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
    internal partial class ConfigurationEditorWindow : Window
    {
        ConfigurationEditorWindowViewModel _viewModel;

        internal ConfigurationEditorWindow( IConfigurationManager configurationManager, LabStateManager serviceInfoManager )
        {
            this.Closed += ConfigurationEditorWindow_Closed;
            _viewModel = new ConfigurationEditorWindowViewModel( this, configurationManager, serviceInfoManager );

            this.DataContext = _viewModel;

            InitializeComponent();
        }

        void ConfigurationEditorWindow_Closed( object sender, EventArgs e )
        {
            _viewModel.Dispose();
            this.DataContext = null;
        }

        private void CloseButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }
    }

 
}
