﻿using System;
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
using Yodii.DemoApp.Plugins.Views;

namespace Yodii.DemoApp.Examples.Plugins.Views
{
    /// <summary>
    /// Interaction logic for Client1.xaml
    /// </summary>
    public partial class Company1View : Window
    {
        public Company1 ViewModel
        {
            get
            {
                return (Company1)this.DataContext;
            }
        }

        public Company1View()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;

            Left = SystemParameters.PrimaryScreenWidth - ( Width + 450 );
            Top = SystemParameters.FullPrimaryScreenHeight - ( Height + 350 );
            MinHeight = MinWidth = MaxHeight = MaxWidth = 550;
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            Window w = new AddProductWindow();
            w.DataContext = DataContext;
            w.Show();
        }
    }
}
