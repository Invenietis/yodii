using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _market;

        public Client1( IMarketPlaceService market )
            : base( true )
        {
            _market = market;

        }

        void CheckNewProducts()
        {
            _market.CheckNewProducts();
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
