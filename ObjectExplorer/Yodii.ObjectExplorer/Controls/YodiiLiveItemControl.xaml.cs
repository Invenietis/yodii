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
using System.Windows.Navigation;
using System.Windows.Shapes;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;

namespace Yodii.ObjectExplorer.Controls
{
    /// <summary>
    /// Interaction logic for YodiiLiveItemControl.xaml
    /// </summary>
    public partial class YodiiLiveItemControl : UserControl
    {
        public YodiiLiveItemControl()
        {
            InitializeComponent();
        }

        public YodiiItemViewModelBase LiveItemViewModel
        {
            get { return (YodiiItemViewModelBase)GetValue( LiveItemViewModelProperty ); }
            set { SetValue( LiveItemViewModelProperty, value ); }
        }

        public static readonly DependencyProperty LiveItemViewModelProperty = 
            DependencyProperty.Register( "LiveItemViewModel", typeof( YodiiItemViewModelBase ), typeof( YodiiLiveItemControl ), new PropertyMetadata( default( YodiiItemViewModelBase ) ) );


    }
}
