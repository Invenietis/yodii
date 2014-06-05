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
    /// Interaction logic for Plugin2Window.xaml
    /// </summary>
    public partial class Plugin2Window : Window
    {
        MainWindowViewModelPlugin2 _vm;

        public Plugin2Window()
        {
            _vm = new MainWindowViewModelPlugin2();
            this.DataContext = _vm;

            //InitializeComponent();
        }
    }
}
