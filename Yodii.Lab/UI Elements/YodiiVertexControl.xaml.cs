using System;
using System.Collections.Generic;
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
            typeof( YodiiVertexControl )
            );
        public static readonly DependencyProperty ConfigurationManagerProperty = 
            DependencyProperty.Register( "ConfigurationManager", typeof( ConfigurationManager ),
            typeof( YodiiVertexControl )
            );

        public YodiiVertexControl()
        {
            InitializeComponent();
        }

        public YodiiGraphVertex Vertex
        {
            get { return (YodiiGraphVertex)GetValue( VertexProperty ); }
            set { SetValue( VertexProperty, value ); }
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
            Vertex.RemoveSelf();
        }
    }
}
