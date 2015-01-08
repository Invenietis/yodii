#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\UI Elements\ServicePropertyPanel.xaml.cs) is part of CiviKey. 
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
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Lab.Utils;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for ServicePropertyPanel.xaml
    /// </summary>
    internal partial class ServicePropertyPanel : UserControl
    {
        #region Fields
        bool _resettingGeneralizationComboBox = false;
        #endregion

        #region Dependency properties

        public static readonly DependencyProperty LiveServiceInfoProperty = 
            DependencyProperty.Register( "LiveServiceInfo", typeof( LabServiceInfo ),
            typeof( ServicePropertyPanel ), new PropertyMetadata( LiveServiceInfoChanged )
            );

        public static readonly DependencyProperty ServiceInfoManagerProperty = 
            DependencyProperty.Register( "ServiceInfoManager", typeof( LabStateManager ),
            typeof( ServicePropertyPanel )
            );

        #endregion

        public ServicePropertyPanel()
        {
            InitializeComponent();
        }

        #region Dependency property change handlers
        private static void LiveServiceInfoChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ServicePropertyPanel p = d as ServicePropertyPanel;
            if( e.NewValue == null )
            {
                // Explicit binding removal is necessary here, or WPF will null the ComboBox binding, effectively removing ServiceInfo.Generalization.
                p.GeneralizationComboBox.ClearValue( ComboBox.SelectedValueProperty );
            }
            // May fire SelectionChanged when switching LiveInfos

            p._resettingGeneralizationComboBox = true;

            p.InvalidateVisual();

        }
        #endregion

        #region Local property getter/setters
        internal LabStateManager ServiceInfoManager
        {
            get { return (LabStateManager)GetValue( ServiceInfoManagerProperty ); }
            set { SetValue( ServiceInfoManagerProperty, value ); }
        }

        public LabServiceInfo LiveServiceInfo
        {
            get { return (LabServiceInfo)GetValue( LiveServiceInfoProperty ); }
            set { SetValue( LiveServiceInfoProperty, value ); }
        }
        #endregion

        #region Event handlers
        private void ServiceNamePropertyTextBox_LostFocus( object sender, RoutedEventArgs e )
        {
            if( LiveServiceInfo == null ) return;
            UpdateServicePropertyNameWithTextbox( sender as System.Windows.Controls.TextBox );
        }

        private void ServiceNamePropertyTextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( LiveServiceInfo == null ) return;
            if( e.Key == System.Windows.Input.Key.Enter )
            {
                UpdateServicePropertyNameWithTextbox( sender as System.Windows.Controls.TextBox );
            }
        }

        private void UpdateServicePropertyNameWithTextbox( System.Windows.Controls.TextBox textBox )
        {
            if( LiveServiceInfo == null ) return;
            LabServiceInfo liveService = textBox.DataContext as LabServiceInfo;

            if( liveService.ServiceInfo.ServiceFullName == textBox.Text ) return;
            if( String.IsNullOrWhiteSpace( textBox.Text ) ) return;

            DetailedOperationResult r = ServiceInfoManager.RenameService( liveService.ServiceInfo, textBox.Text );

            if( !r )
            {
                textBox.Text = liveService.ServiceInfo.ServiceFullName;
                MessageBox.Show( String.Format( "Couldn't change service name.\n{0}", r.Reason ), "Couldn't change service name", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK );
            }
        }

        private void ClearGeneralizationButton_Click( object sender, RoutedEventArgs e )
        {
            if( LiveServiceInfo == null ) return;
            if( LiveServiceInfo.IsLive ) return;
            _resettingGeneralizationComboBox = true;
            LiveServiceInfo.ServiceInfo.Generalization = null;

            InvalidateVisual();
        }

        private void GeneralizationComboBox_SelectionChanged( object sender, SelectionChangedEventArgs e )
        {
            if( LiveServiceInfo == null ) return;

            ComboBox box = sender as ComboBox;

            if( _resettingGeneralizationComboBox )
            {
                _resettingGeneralizationComboBox = false;
                box.GetBindingExpression( ComboBox.SelectedValueProperty ).UpdateTarget();
                return;
            }

            ServiceInfo newGeneralization = (ServiceInfo)box.SelectedValue;

            if( newGeneralization == null ) return;
            if( newGeneralization == LiveServiceInfo.ServiceInfo.Generalization ) return;

            if( newGeneralization.SpecializesService(this.LiveServiceInfo.ServiceInfo))
            {
                MessageBox.Show(
                    String.Format( "Service {0} is already a generalization of service {1}.\nChanging the generalization of {0} to {1} would create a loop.", this.LiveServiceInfo.ServiceInfo.ServiceFullName, newGeneralization.ServiceFullName ),
                    "Cannot use this generalization",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop,
                    MessageBoxResult.OK
                    );

                _resettingGeneralizationComboBox = true;
            }
            else if( newGeneralization == this.LiveServiceInfo.ServiceInfo )
            {
                MessageBox.Show(
                    String.Format( "A service cannot specialize itself." ),
                    "Cannot use this generalization",
                    MessageBoxButton.OK,
                    MessageBoxImage.Stop,
                    MessageBoxResult.OK
                    );
                _resettingGeneralizationComboBox = true;
            }
            else
            {
                this.LiveServiceInfo.ServiceInfo.Generalization = newGeneralization;
            }

            box.GetBindingExpression( ComboBox.SelectedValueProperty ).UpdateTarget();
        }
        #endregion
    }
}
