using System;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin, IConsumer
    {
        readonly IMarketPlaceService _market;
        readonly string _name;
        readonly string _adress;
        ObservableCollection<MarketPlace.Product> _purchasedProducts;

        public Client1( IMarketPlaceService market, string name, string adress )
            : base( true )
        {
            _market = market;
            _name = name;
            _adress = adress;
            _purchasedProducts = new ObservableCollection<MarketPlace.Product>();
        }

        public void Buy()
        {
            _market.CheckNewProducts( this );
        }

        public void ReceiveDelivery( MarketPlace.Product purchasedProduct )
        {
            _purchasedProducts.Add( purchasedProduct );
            RaisePropertyChanged();
        }

        public string Name { get { return _name; } }

        public string Adress { get { return _adress; } }

        public IClientInfo Info { get { return new ClientInfo( _name, _adress ); } }

        protected override Window CreateWindow()
        {
            Window = new Client1View()
            {
                DataContext = this
            };

            return Window;
        }
    }
}
