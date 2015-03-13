using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yodii.ObjectExplorer.Windows
{
    /// <summary>
    /// Interaction logic for ObjectExplorerWindow.xaml
    /// </summary>
    public partial class ObjectExplorerWindow : Window
    {
        public ObjectExplorerWindow()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.ObjectExplorerViewModel();
        }
    }
}
