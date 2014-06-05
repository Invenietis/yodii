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

namespace Yodii.DemoApp
{
    /// <summary>
    /// Interaction logic for Plugin1Window.xaml
    /// </summary>
    public partial class Plugin1Window : Window
    {
        MainWindowViewModelPlugin1 _vm;

        public Plugin1Window()
        {
            _vm = new MainWindowViewModelPlugin1();
            this.DataContext = _vm;

            //InitializeComponent();
        }
    }
}
