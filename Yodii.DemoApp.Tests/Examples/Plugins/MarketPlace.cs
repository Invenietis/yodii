using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        readonly Client1 _client;
        readonly Company1 _company;
        readonly ITimerService _timer;

        public MarketPlace( Client1 client, Company1 company, ITimerService timer, bool runningLifetimeWindow, Window window )
            : base( runningLifetimeWindow, window ) 
        {
            _client = client;
            _company = company;
            _timer = timer;
        }

        public void CheckNewProducts()
        {
        }

        protected override Window CreateAndShowWindow()
        {
            throw new NotImplementedException();
        }

        protected override void DestroyWindow()
        {
            throw new NotImplementedException();
        }
    }
}
