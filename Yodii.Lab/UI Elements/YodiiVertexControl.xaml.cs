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

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for YodiiVertexControl.xaml
    /// </summary>
    public partial class YodiiVertexControl : UserControl
    {

        public static readonly DependencyProperty VertexProperty = 
            DependencyProperty.Register( "Vertex", typeof( YodiiGraphVertex ),
            typeof( YodiiVertexControl ), new FrameworkPropertyMetadata(OnVertexChangedCallback)
            );

        private static void OnVertexChangedCallback( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            (d as YodiiVertexControl).OnNewVertex();
        }

        public static readonly DependencyProperty ConfigurationManagerProperty = 
            DependencyProperty.Register( "ConfigurationManager", typeof( ConfigurationManager ),
            typeof( YodiiVertexControl )
            );

        public YodiiVertexControl()
        {
            InitializeComponent();
            ConfigurationStatusMenu.ItemContainerGenerator.StatusChanged += ItemContainerGenerator_StatusChanged;
        }

        void ItemContainerGenerator_StatusChanged( object sender, EventArgs e )
        {
            if(ConfigurationStatusMenu.ItemContainerGenerator.Status == System.Windows.Controls.Primitives.GeneratorStatus.ContainersGenerated)
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

        public ConfigurationManager ConfigurationManager
        {
            get { return (ConfigurationManager)GetValue( ConfigurationManagerProperty ); }
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
            // If plugin: PluginInfo.PluginId / else: ServiceInfo.ServiceFullName
            string pluginOrServiceId = Vertex.IsPlugin ? Vertex.LivePluginInfo.PluginInfo.PluginId.ToString() : Vertex.LiveServiceInfo.ServiceInfo.ServiceFullName;

            // TODO: Better handling of config change. Current implementation: remove all matching entries, then add the new one.
            foreach( ConfigurationLayer layer in ConfigurationManager.Layers )
            {
                foreach( var item in layer.Items.ToList() )
                {
                    if( item.ServiceOrPluginId == pluginOrServiceId )
                    {
                        layer.Items.Remove( pluginOrServiceId );
                    }
                }
            }

            ConfigurationLayer changedLayer;
            if( ConfigurationManager.Layers.Count == 0 )
            {
                changedLayer = new ConfigurationLayer( "Auto-added layer" );
                ConfigurationManager.Layers.Add( changedLayer );
            }
            else
            {
                changedLayer = ConfigurationManager.Layers.First();
            }

            changedLayer.Items.Add( pluginOrServiceId, status, "Right-click change" );
        }

        private void DeleteMenuItem_Click( object sender, RoutedEventArgs e )
        {
            if( Vertex == null ) return;
            Vertex.RemoveSelf();
        }
    }
}
