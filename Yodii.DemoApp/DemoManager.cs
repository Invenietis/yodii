using System;
using System.Collections.Generic;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public sealed class DemoManager
    {
        ITimerService _mainTimer;
        IMarketPlaceService _marketPlace;
        ICarRepairService _carRepair;
        IOutSourcingService _outsourcing;
        IDeliveryService _delivery;
        //string[,,] _data;
        List<Client1> _clients;
        List<Company1> _companies;
        
        string[,] _clientsData;
        string[] _companyData;
        List<Tuple<string,string>> _test;
        readonly Random _r;
        //StandardDiscoverer _standardDiscoverer;

        public DemoManager()
        {
            _clients = new List<Client1>();
            _companies = new List<Company1>();
            _mainTimer = new TimerHandler();
            _marketPlace = new MarketPlace();
            _carRepair = new Garage();
            _outsourcing = new ManPower();
            //_delivery = new LivrExpress( _carRepair, _outsourcing, _marketPlace );
            _delivery = new LaPoste( _marketPlace, _mainTimer,_outsourcing );
            _r = new Random();
        }

        public void Initialize()
        {            
            _clientsData = new string[,]
            {             
                {"Buyer One","1st Street"},
                {"Buyer Two","2nd Street"},
                {"Buyer Three","3rd Street"},
                {"Buyer Four","4th Street"},
                {"Buyer Five","5th Street"},
                {"Buyer Six","6th Street"},
                {"Buyer Seven","7th Street"},
                {"Buyer Height","8th Street"},
                {"Buyer Nine","9th Street"},
                {"Buyer Ten","10th Street"},             
            };

            //_test = new List<Tuple<string,string>>
            //{
            //    "Buyer One","1st Street",
            //    {"Buyer Two","2nd Street"},
            //    {"Buyer Three","3rd Street"},
            //    {"Buyer Four","4th Street"},
            //    {"Buyer Five","5th Street"},
            //    {"Buyer Six","6th Street"},
            //    {"Buyer Seven","7th Street"},
            //    {"Buyer Height","8th Street"},
            //    {"Buyer Nine","9th Street"},
            //    {"Buyer Ten","10th Street"},             
            //};
            _companyData = new string[]
            {
                "Amazon",
                "eBay",
                "Google",
                "Apple",
                "Microsoft",
                "IBM",
                "LG",
                //...
            };
           
            //_standardDiscoverer = new StandardDiscoverer();
        }

        public bool Start()
        {
            return Start( _r.Next( 2, 5 ), _r.Next( 5, 10 ) );
        }

        public bool Start( int nbCompanies, int nbClients )
        {
            if( !( nbClients > 0 || nbCompanies > 0 ) ) return false;

            for( int i = 0; i < nbClients; i++ )
            {
                //_clients.Add( new Client1( _marketPlace, _clientsData.GetValue(i)., _clientsData.GetValue(i)
            }
            _clients.Add( new Client1( _marketPlace, "Chuck Norris", "15th Street" ) );
            _companies.Add( new Company1( _marketPlace, _delivery, "MyCompany" ) );




            //_standardDiscoverer.ReadAssembly( System.IO.Path.GetFullPath( "Yodii.DemoApp.exe" ) );
            //_discoveredInfo = _standardDiscoverer.GetDiscoveredInfo();
            _companies[0].AddNewProduct( "lol", ProductCategory.Entertainment, 10 );
            _companies[0].AddNewProduct( "lal", ProductCategory.Entertainment, 10 );
            _companies[0].AddNewProduct( "lul", ProductCategory.Entertainment, 10 );


            //_mainTimer.Start();

            //_companies[0].AddNewProduct( );

            //_companies[0].Products[0].Name = "COUCOU";

            ( (IYodiiPlugin)_clients[0] ).Start();
            ( (IYodiiPlugin)_companies[0] ).Start();
            ((IYodiiPlugin) _delivery).Start();

            return true;
        }

        public int CompaniesCount { get { return _companies.Count; } }

        public int ClientsCount { get { return _clients.Count; } }
    }
}
