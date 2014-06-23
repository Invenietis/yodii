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
        }

        private void Buy_Button_Click( object sender, RoutedEventArgs e )
        {
            ( (Client1)DataContext ).Buy();
        }
    }
}
