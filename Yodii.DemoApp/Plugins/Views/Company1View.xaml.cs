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
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            //ViewModel.AddNewProduct( "ergrgrgr" );
        }
    }
}
