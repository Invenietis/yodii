using System;
using System.Collections.Generic;
using Yodii.Model;
using Yodii.Engine;
using Yodii.Discoverer;
using Yodii.Host;
using System.IO;

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

        string[] _companyData;
        List<Tuple<string, string>> _clientsData;
        //StandardDiscoverer _standardDiscoverer;

        public DemoManager()
        {
            /*_clients = new List<Client1>();
            _companies = new List<Company1>();
            _mainTimer = new TimerHandler();
            _marketPlace = new MarketPlace();
            _carRepair = new Garage();
            _outsourcing = new ManPower();
            //_delivery = new LivrExpress( _carRepair, _outsourcing, _marketPlace );
            _delivery = new LaPoste( _marketPlace, _mainTimer, _outsourcing );*/
        }

        public void Initialize()
        {
          /* _clientsData = new List<Tuple<string, string>>
           {
                new Tuple<string,string>("Buyer One","1st Street"),
                new Tuple<string,string>("Buyer Two","2nd Street"),
                new Tuple<string,string>("Buyer Three","3rd Street"),
                new Tuple<string,string>("Buyer Four","4th Street"),
                new Tuple<string,string>("Buyer Five","5th Street"),
                new Tuple<string,string>("Buyer Six","6th Street"),
                new Tuple<string,string>("Buyer Seven","7th Street"),
                new Tuple<string,string>("Buyer Height","8th Street"),
                new Tuple<string,string>("Buyer Nine","9th Street"),
                new Tuple<string,string>("Buyer Ten","10th Street"),      
            };

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
            };*/
            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.DemoApp.exe" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();

            YodiiHost host = new YodiiHost();
            YodiiEngine engine = new YodiiEngine( host );
            engine.SetDiscoveredInfo( info );
            IConfigurationLayer cl = engine.Configuration.Layers.Create();
            cl.Items.Add( "Yodii.DemoApp.Client1", ConfigurationStatus.Running );
            cl.Items.Add( "Yodii.DemoApp.Company1", ConfigurationStatus.Running );
            engine.StartEngine();
            //engine.LiveInfo.FindPlugin( "Yodii.DemoApp.Client1" ).Start();
            //engine.LiveInfo.FindPlugin( "Yodii.DemoApp.Company1" ).Start();

        }

        public bool Start()
        {
            Random r = new Random();
            return Start( r.Next( 2, 5 ), r.Next( 1, 7 ) );
        }

        public bool Start( int nbCompanies, int nbClients )
        {
            /*if( !(nbClients > 0 || nbCompanies > 0) ) return false;

            Generate( nbClients, nbCompanies );

            #region DiscovererCode
            //_standardDiscoverer.ReadAssembly( System.IO.Path.GetFullPath( "Yodii.DemoApp.exe" ) );
            //_discoveredInfo = _standardDiscoverer.GetDiscoveredInfo();
            #endregion

            //for( int i = 0; i < _clients.Count; i++ )
            //{
            //    ( (IYodiiPlugin)_clients[i] ).Start();
            //}
            //for( int y = 0; y < _companies.Count; y++ )
            //{
            //    ( (IYodiiPlugin)_companies[y] ).Start();
            //}
            ((IYodiiPlugin)_clients[0]).Start();
            ((IYodiiPlugin)_companies[0]).Start();
            ((IYodiiPlugin)_delivery).Start();
            ((IYodiiPlugin)_outsourcing).Start();
            */
            return true;
        }

        private void Generate( int nbClients, int nbCompanies )
        {
           /* for( int i = 0; i < nbClients; i++ )
            {
                _clients.Add( new Client1( _marketPlace, _clientsData[i].Item1, _clientsData[i].Item2 ) );
            }

            for( int i = 0; i < nbCompanies; i++ )
            {
                _companies.Add( new Company1( _marketPlace, _delivery, _companyData[i] ) );
            }*/
        }

        public int CompanyFactoryCount { get { return _companyData.GetLength( 0 ); } }

        public int ClientFactoryCount { get { return _clientsData.Count; } }
    }
}
