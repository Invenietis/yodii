using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _market;
        readonly ITimerService _timer;

        public Client1( IMarketPlaceService market, ITimerService timer, bool runningLifetimeWindow, Client1View window )
            : base( runningLifetimeWindow, window )
        {
            _timer = timer;
            _market = market;
        }

        void BuyNewProduct()
        {
            _market.CheckNewProducts();
        }

        protected override Window CreateAndShowWindow()
        {
            Client1View view = new Client1View()
            {
                DataContext = this
            };

            view.Show();
            return view;
        }

        protected override void DestroyWindow()
        {
            throw new NotImplementedException();
        }
    }
}
