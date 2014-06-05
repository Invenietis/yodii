using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        Client1 _client;
        Company1 _company;
        ITimerService _timer;

        public MarketPlace( bool runningLifetimeWindow, Client1 client, Company1 company, ITimerService timer )
            : base( runningLifetimeWindow ) 
        {
            _client = client;
            _company = company;
            _timer = timer;
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
