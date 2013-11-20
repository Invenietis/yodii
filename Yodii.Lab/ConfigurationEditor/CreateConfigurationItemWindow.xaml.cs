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
using System.Windows.Shapes;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab.ConfigurationEditor
{
    /// <summary>
    /// Interaction logic for CreateConfigurationItemWindow.xaml
    /// </summary>
    public partial class CreateConfigurationItemWindow : Window
    {
        internal readonly CreateConfigurationItemWindowViewModel ViewModel;

        public CreateConfigurationItemWindow(ServiceInfoManager serviceManager)
        {
            Debug.Assert( serviceManager != null );

            ViewModel = new CreateConfigurationItemWindowViewModel( serviceManager );
            this.DataContext = ViewModel;

            InitializeComponent();

            ServicesDropDownButton.Focus();
        }

        private void OkButton_Click( object sender, RoutedEventArgs e )
        {
            if( ViewModel.SelectedItem == null ) return;
            this.DialogResult = true;
            this.Close();
        }

    }
}
