#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.Wpf\UI Elements\YodiiVertexControl.xaml.cs) is part of CiviKey. 
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
using Yodii.Model;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Interaction logic for YodiiVertexControl.xaml
    /// </summary>
    internal partial class YodiiVertexControl : UserControl
    {

        public static readonly DependencyProperty VertexProperty = 
            DependencyProperty.Register( "Vertex", typeof( YodiiGraphVertex ),
            typeof( YodiiVertexControl ), new FrameworkPropertyMetadata( OnVertexChangedCallback )
            );

        private static void OnVertexChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            (d as YodiiVertexControl).OnNewVertex();
        }

        public static readonly DependencyProperty ConfigurationManagerProperty = 
            DependencyProperty.Register( "ConfigurationManager", typeof( IConfigurationManager ),
            typeof( YodiiVertexControl )
            );

        public YodiiVertexControl()
        {
            InitializeComponent();
            ConfigurationStatusMenu.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        void ItemContainerGenerator_StatusChanged( object sender, EventArgs e )
        {
            if( ConfigurationStatusMenu.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated )
            {
                UpdateCheckbox();
            }
        }

        public YodiiGraphVertex Vertex
        {
            get { return (YodiiGraphVertex)GetValue( VertexProperty ); }
            set { SetValue( VertexProperty, value ); }
        }

        private void OnNewVertex()
        {
            if( Vertex == null ) return;

            Vertex.PropertyChanged += Vertex_PropertyChanged;
            UpdateCheckbox();
        }

        void Vertex_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            if( Vertex == null ) return;
            Debug.Assert( Vertex == sender as YodiiGraphVertex );

            UpdateCheckbox();
        }

        void UpdateCheckbox()
        {
            if( Vertex == null ) return;
            if( Vertex.HasConfiguration == false )
                SetCheckedConfigurationStatus( null );
            else
                SetCheckedConfigurationStatus( Vertex.ConfigurationStatus );
        }

        void SetCheckedConfigurationStatus( ConfigurationStatus? newStatus )
        {
            if( ConfigurationStatusMenu.ItemContainerGenerator.Status != System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated ) return;
            foreach( var item in this.ConfigurationStatusMenu.Items )
            {
                MenuItem childItem = ConfigurationStatusMenu.ItemContainerGenerator.ContainerFromItem( item ) as MenuItem;

                if( newStatus == null )
                {
                    childItem.IsChecked = false;
                    continue;
                }
                else
                {
                    ConfigurationStatus status = (ConfigurationStatus)item;
                    ConfigurationStatus childStatus = (ConfigurationStatus)childItem.DataContext;
                    if( childStatus == newStatus )
                        childItem.IsChecked = true;
                    else
                        childItem.IsChecked = false;
                }
            }
        }

        public IConfigurationManager ConfigurationManager
        {
            get { return (IConfigurationManager)GetValue( ConfigurationManagerProperty ); }
            set { SetValue( ConfigurationManagerProperty, value ); }
        }

        private void ConfigurationStatusMenuItem_Click( object sender, RoutedEventArgs e )
        {
            MenuItem item = e.OriginalSource as MenuItem;
            ConfigurationStatus status = (ConfigurationStatus)item.DataContext;

            ChangeConfiguration( status );
        }

        private void ChangeConfiguration( ConfigurationStatus status )
        {
            if( Vertex == null ) return;

            string pluginOrServiceId = Vertex.IsPlugin ? Vertex.LivePluginInfo.PluginInfo.PluginFullName : Vertex.LiveServiceInfo.ServiceInfo.ServiceFullName;

            // TODO: Better handling of config change. Current implementation: remove all matching entries, then add the new one.
            foreach( IConfigurationLayer layer in ConfigurationManager.Layers )
            {
                foreach( var item in layer.Items.ToList() )
                {
                    if( item.ServiceOrPluginFullName == pluginOrServiceId )
                    {
                        layer.Items.Remove( pluginOrServiceId );
                    }
                }
            }

            // Remove configuration when selecting Optional.
            if( status == ConfigurationStatus.Optional ) return;

            IConfigurationLayer changedLayer;
            if( ConfigurationManager.Layers.Count == 0 )
            {
                changedLayer = ConfigurationManager.Layers.Create( "DefaultLayer" );
            }
            else
            {
                changedLayer = ConfigurationManager.Layers.First();
            }

            var result = changedLayer.Items.Set( pluginOrServiceId, status, "Right-click change" );

            if( !result.Success )
            {
                MessageBox.Show( String.Format( "Could not set {0} to {2}, as it would cause the following error:\n\n{1}",
                    pluginOrServiceId,
                    result.Describe(),
                    status.ToString() ), "Couldn't set item", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK
                    );
            }
        }
    }
}
