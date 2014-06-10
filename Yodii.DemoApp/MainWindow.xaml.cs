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

namespace Yodii.DemoApp
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        readonly ITimerService _mainTimer;
        readonly IMarketPlaceService _marketPlace;
        readonly ICarRepairService _carRepair;
        readonly IOutSourcingService _outsourcing;
        readonly IDeliveryService _delivery;

        readonly List<Client1> _clients;
        readonly List<Company1> _companies;

        public MainWindow()
        {
            _clients = new List<Client1>();
            _companies = new List<Company1>();
            _mainTimer = new TimerHandler();
            _marketPlace = new MarketPlace( _mainTimer );
            _carRepair = new Garage( _mainTimer );
            _outsourcing = new ManPower( _mainTimer );
            _delivery = new LivrExpress( _carRepair, _outsourcing, _mainTimer );

            _clients.Add( new Client1( _marketPlace, _mainTimer ) );
            _companies.Add( new Company1( _marketPlace, _delivery, _mainTimer ) );

            InitializeComponent();
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            _mainTimer.Start();
            
            for( int i = 0; i < _clients.Count; i++ )
            {
                ( (IYodiiPlugin)_clients[i] ).Start();
                ( (IYodiiPlugin)_companies[i] ).Start();
            }
        }
    }
}
