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
using System.Windows.Shapes;

namespace Yodii.Lab.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for CreateConfigurationLayerWindow.xaml
    /// </summary>
    public partial class CreateConfigurationLayerWindow : Window
    {
        public CreateConfigurationLayerWindow()
        {
            InitializeComponent();
            this.NewLayerName.Focus();
        }

        private void OkButton_Click( object sender, RoutedEventArgs e )
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
