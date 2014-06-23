using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
using Yodii.Discoverer;
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
        readonly StandardDiscoverer _standardDiscoverer;
        IDiscoveredInfo _discoveredInfo;

        public MainWindow()
        {
            _clients = new List<Client1>();
            _companies = new List<Company1>();
            _mainTimer = new TimerHandler();
            _marketPlace = new MarketPlace( );
            _carRepair = new Garage( );
            _outsourcing = new ManPower(  );
            _delivery = new LivrExpress( _carRepair, _outsourcing, _marketPlace );

            _clients.Add( new Client1( _marketPlace, "Chuck Norris", "15th Street" ) );
            _companies.Add( new Company1( _marketPlace, _delivery ) );
            _standardDiscoverer = new StandardDiscoverer();
            InitializeComponent();
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
            _standardDiscoverer.ReadAssembly( System.IO.Path.GetFullPath( "Yodii.DemoApp.exe" ) );
            _discoveredInfo = _standardDiscoverer.GetDiscoveredInfo();
            
            _mainTimer.Start();

            //_companies[0].AddNewProduct( );

            //_companies[0].Products[0].Name = "COUCOU";

            ( (IYodiiPlugin)_clients[0] ).Start();
            ( (IYodiiPlugin)_companies[0] ).Start();
        }
    }
}
