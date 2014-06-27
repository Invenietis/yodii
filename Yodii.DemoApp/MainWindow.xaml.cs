using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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
using Yodii.Discoverer;
using Yodii.Model;

namespace Yodii.DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //readonly ITimerService _mainTimer;
        //readonly IMarketPlaceService _marketPlace;
        //readonly ICarRepairService _carRepair;
        //readonly IOutSourcingService _outsourcing;
        //readonly IDeliveryService _delivery;

        //readonly List<Client1> _clients;
        //readonly List<Company1> _companies;
        //readonly StandardDiscoverer _standardDiscoverer;
        //IDiscoveredInfo _discoveredInfo;
        readonly DemoManager _manager;

        public MainWindow()
        {
            _manager = new DemoManager();
            _manager.Initialize();
            InitializeComponent();
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            if( !( string.IsNullOrEmpty( nbClients.Text ) && ( string.IsNullOrEmpty( nbCompanies.Text ) ) ) )
            {
                int clientCount = int.Parse( nbClients.Text );
                int companyCount = int.Parse( nbCompanies.Text );
                if( clientCount > 0 && clientCount < _manager.ClientFactoryCount && companyCount > 0 && companyCount < _manager.CompanyFactoryCount )
                    _manager.Start( companyCount, clientCount );
            }
            else
                _manager.Start();
        }

        private void NumberValidationTextBox( object sender, TextCompositionEventArgs e )
        {
            Regex r = new Regex( "[^0-9]+" );
            e.Handled = r.IsMatch( e.Text );
        }
    }
}