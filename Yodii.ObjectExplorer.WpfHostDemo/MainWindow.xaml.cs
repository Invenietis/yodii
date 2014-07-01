using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Host;
using Yodii.Model;

namespace Yodii.ObjectExplorer.WpfHostDemo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        IYodiiEngine _engine;

        public MainWindow()
        {
            ObjectExplorerManager m = new ObjectExplorerManager();
            _engine = m.Engine;

            m.SetDiscoveredInfo();

            this.DataContext = this;

            InitializeComponent();
        }

        IYodiiEngine Engine { get { return _engine; } }

        private void Start_Click( object sender, RoutedEventArgs e )
        {
            Engine.Start();
        }

        private void Stop_Click( object sender, RoutedEventArgs e )
        {
            Engine.Stop();
        }
    }
}
