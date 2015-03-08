#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\UI Elements\PluginPropertyPanel.xaml.cs) is part of CiviKey. 
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
using System.ComponentModel;
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
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for PluginPropertyPanel.xaml
    /// </summary>
    internal partial class PluginPropertyPanel : UserControl
    {
        #region Fields

        #endregion

        public PluginPropertyPanel()
        {
            InitializeComponent();
        }

        #region Dependency properties

        public static readonly DependencyProperty LivePluginInfoProperty = 
            DependencyProperty.Register( "LivePluginInfo", typeof( LabPluginInfo ),
            typeof( PluginPropertyPanel ), new FrameworkPropertyMetadata( DependencyPropertyChanged )
            );

        public static readonly DependencyProperty ServiceInfosProperty = 
            DependencyProperty.Register( "ServiceInfos", typeof( ICKObservableReadOnlyCollection<IServiceInfo> ),
            typeof( PluginPropertyPanel )
            );

        #endregion

        #region Local dependency properties getters/setters

        public ICKObservableReadOnlyCollection<IServiceInfo> ServiceInfos
        {
            get { return (ICKObservableReadOnlyCollection<IServiceInfo>)GetValue( ServiceInfosProperty ); }
            set { SetValue( ServiceInfosProperty, value ); }
        }

        public LabPluginInfo LivePluginInfo
        {
            get { return (LabPluginInfo)GetValue( LivePluginInfoProperty ); }
            set { SetValue( LivePluginInfoProperty, value ); }
        }
        #endregion

        #region Command handlers
        public void ExecuteClearService( object param )
        {
            if( LivePluginInfo == null ) return;

            LivePluginInfo.PluginInfo.Service = null;
        }
        #endregion

        #region Property change handlers
        private static void DependencyPropertyChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            PluginPropertyPanel p = d as PluginPropertyPanel;
            switch( e.Property.Name )
            {
                case "LivePluginInfo":
                    if( e.NewValue == null )
                    {
                        // Explicit binding removal is necessary here, or WPF will null the ComboBox binding, effectively removing PluginInfo.Service.
                        // Will no longer work once set at null?
                        p.PluginServiceComboBox.ClearValue( ComboBox.SelectedValueProperty );
                    }
                    break;
            }
        }
        #endregion

        #region Event handlers
        private void DeleteReferenceButton_Click( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement;
            MockServiceReferenceInfo serviceRef = parentElement.DataContext as MockServiceReferenceInfo;

            serviceRef.Owner.InternalServiceReferences.Remove( serviceRef );
        }

        private void ReferenceRequirementComboBox_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
        {
            if( e.RemovedItems.Count == 1 && e.AddedItems.Count == 1 )
            {
                ComboBox box = sender as ComboBox;
                FrameworkElement parentElement = box.Parent as FrameworkElement;
                MockServiceReferenceInfo serviceRef = parentElement.DataContext as MockServiceReferenceInfo;

                DependencyRequirement oldReq = serviceRef.Requirement;
                DependencyRequirement newReq = (DependencyRequirement)e.AddedItems[0];

                serviceRef.Requirement = newReq;
            }
        }

        private void CreateReferenceButton_Click( object sender, RoutedEventArgs e )
        {
            if( LivePluginInfo == null ) return;

            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement;

            ComboBox requirementComboBox = parentElement.FindName( "NewReferenceRequirementComboBox" ) as ComboBox;
            ComboBox serviceComboBox = parentElement.FindName( "NewReferenceServiceComboBox" ) as ComboBox;

            ServiceInfo service = serviceComboBox.SelectedItem as ServiceInfo;
            DependencyRequirement req = (DependencyRequirement)requirementComboBox.SelectedItem;
            if( LivePluginInfo.PluginInfo.CanReference( service ) )
            {
                LivePluginInfo.PluginInfo.InternalServiceReferences.Add( new MockServiceReferenceInfo( LivePluginInfo.PluginInfo, service, req ) );
            }
            else
            {
                MessageBox.Show( "You cannot reference this service.\nMake sure the selected service is in another family, or in the same family branch.", "Reference failed" );
            }
        }

        private void ClearServiceButton_Click( object sender, RoutedEventArgs e )
        {
            if( LivePluginInfo == null ) return;


            LivePluginInfo.PluginInfo.Service = null;
        }
        #endregion


    }
}
