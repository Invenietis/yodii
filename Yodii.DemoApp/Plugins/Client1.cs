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
        ObservableCollection<MarketPlace.Product> _purchasedProducts;

        public Client1( IMarketPlaceService market, string name )
            : base( true )
        {
            _market = market;
            _name = name;
            _purchasedProducts = new ObservableCollection<MarketPlace.Product>();
        }

        public void BuyNewProducts()
        {
            _market.CheckNewProducts( this );
        }

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
