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
        readonly ClientInfo _clientInfo;
        ObservableCollection<MarketPlace.Product> _purchasedProducts;

        public Client1( IMarketPlaceService market, string name, string adress )
            : base( true )
        {
            _market = market;
            _clientInfo = new ClientInfo( name, adress );
            _purchasedProducts = new ObservableCollection<MarketPlace.Product>();
        }

        public void Buy( MarketPlace.Product product = null )
        {
            _market.PlaceOrder( _clientInfo, product );
        }

        public void ReceiveDelivery( MarketPlace.Product purchasedProduct )
        {
            _purchasedProducts.Add( purchasedProduct );
            RaisePropertyChanged();
        }

        public IClientInfo Info { get { return _clientInfo; } }

        public ObservableCollection<MarketPlace.Product> PurchasedProducts { get { return _purchasedProducts; } }

        public ObservableCollection<MarketPlace.Product> AvailableProducts { get { return _market.Products; } }

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
