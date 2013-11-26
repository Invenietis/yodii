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
    public partial class ConfigurationEditorWindow : Window
    {
        internal ConfigurationEditorWindow( IConfigurationManager configurationManager, ServiceInfoManager serviceInfoManager )
        {
            var viewModel = new ConfigurationEditorWindowViewModel( this, configurationManager, serviceInfoManager );

            this.DataContext = viewModel;

            InitializeComponent();
        }

        private void CloseButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }
    }

 
}
