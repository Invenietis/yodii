using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : MonoWindowPlugin, IDeliveryService
    {
        readonly IService<ICarRepairService> _serviceRef1;
        readonly IService<IOutSourcingService> _serviceRef2;
        readonly ITimerService _timer;

        public LivrExpress( IRunningService<ICarRepairService> ServiceRef1, IRunningService<IOutSourcingService> ServiceRef2, ITimerService timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
            _serviceRef1 = ServiceRef1;
            _serviceRef2 = ServiceRef2;
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new LivrExpressView()
            {
                DataContext = this
            };

            Window.Show();
            return Window;
        }

        protected override void DestroyWindow()
        {
            if( Window != null ) Window.Close();
        }
    }
}
