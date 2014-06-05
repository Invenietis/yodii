using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company3 : MonoWindowPlugin
    {
        IMarketPlaceService _serviceRef1;
        IDeliveryService _serviceRef2;
        ITimerService _timer;

        public Company3( IMarketPlaceService ServiceRef1, IDeliveryService ServiceRef2, ITimerService timer, bool runningLifetimeWindow, Window window )
            : base( runningLifetimeWindow, window )
        {
            _serviceRef1 = ServiceRef1;
            _serviceRef2 = ServiceRef2;
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
