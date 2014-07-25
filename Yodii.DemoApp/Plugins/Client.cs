using System;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client : MonoWindowPlugin, IConsumer
    {
        readonly IMarketPlaceService _market;
        readonly ClientInfo _clientInfo;
        ObservableCollection<MarketPlace.Product> _purchasedProducts;

        public Client( IMarketPlaceService market, IYodiiEngine engine )
            : base( true, engine )
        {
            _market = market;
            _clientInfo = new ClientInfo( "Client", "10th Street" );
            _purchasedProducts = new ObservableCollection<MarketPlace.Product>();
        }

        public void Buy( MarketPlace.Product product = null )
        {
            if( product != null && _market.Products.Contains( product ) )
                _market.PlaceOrder( _clientInfo, product );
        }

        public bool ReceiveDelivery( MarketPlace.Product purchasedProduct )
        {
            _purchasedProducts.Add( purchasedProduct );
            RaisePropertyChanged();
            return true;
        }

        public IClientInfo Info { get { return _clientInfo; } }

        public ObservableCollection<MarketPlace.Product> PurchasedProducts { get { return _purchasedProducts; } }

        public ObservableCollection<MarketPlace.Product> AvailableProducts { get { return _market.Products; } }

        protected override Window CreateWindow()
        {
            Window = new ClientView( this )
            {
                DataContext = this
            };
            //_market.Consumers.Add( this );
            return Window;
        }
    }
}
