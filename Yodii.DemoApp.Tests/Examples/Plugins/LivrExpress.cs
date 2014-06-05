using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : MonoWindowPlugin, IDeliveryService
    {
        readonly ICarRepairService _serviceRef1;
        readonly IOutSourcingService _serviceRef2;
        readonly ITimerService _timer;

        public LivrExpress( ICarRepairService ServiceRef1, IOutSourcingService ServiceRef2, ITimerService timer, bool runningLifetimeWindow, Window window )
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
