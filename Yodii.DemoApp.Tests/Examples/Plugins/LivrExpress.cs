using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : MonoWindowPlugin, IDeliveryService
    {
        ICarRepairService _serviceRef1;
        IOutSourcingService _serviceRef2;
        ITimerService _timer;

        public LivrExpress( bool runningLifetimeWindow, ICarRepairService ServiceRef1, IOutSourcingService ServiceRef2, ITimerService timer )
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
