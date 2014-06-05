using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : MonoWindowPlugin
    {
        IMarketPlaceService _serviceRef1;
        IDeliveryService _serviceRef2;
        ITimerService _timer;
        public Company2( bool runningLifetimeWindow, IMarketPlaceService ServiceRef1, IDeliveryService ServiceRef2, ITimerService timer )
            : base( runningLifetimeWindow )
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
