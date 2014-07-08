using System.Windows;

namespace Yodii.DemoApp.Examples.Plugins.Views
{
    /// <summary>
    /// Interaction logic for Client1.xaml
    /// </summary>
    public partial class Client1View : Window
    {
        Client1 _client;
        public Client1View( Client1 client )
        {
            _client = client;
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = 0;
            MinHeight = MinWidth = MaxHeight = MaxWidth = 550;
        }

        private void Buy_Button_Click( object sender, RoutedEventArgs e )
        {
            MarketPlace.Product p = (MarketPlace.Product)ProductGrid.SelectedItem;
            ((Client1)DataContext).Buy( p );
        }
        private void Window_Closing( object sender, System.ComponentModel.CancelEventArgs e )
        {
            e.Cancel = !_client.WindowClosed();
        }
    }
}
