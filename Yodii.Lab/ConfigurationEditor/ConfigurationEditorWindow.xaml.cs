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
