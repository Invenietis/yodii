using System.Windows;

namespace Yodii.DemoApp.Examples.Plugins.Views
{
    /// <summary>
    /// Interaction logic for Client1.xaml
    /// </summary>
    public partial class Client1View : Window
    {
        public Client1View()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = 0;
            MinHeight = MinWidth = MaxHeight = MaxWidth = 550;
        }

        private void Buy_Button_Click( object sender, RoutedEventArgs e )
        {
            MarketPlace.Product p = ( MarketPlace.Product )ProductGrid.SelectedItem;
            ( (Client1)DataContext ).Buy( p );
        }
    }
}
