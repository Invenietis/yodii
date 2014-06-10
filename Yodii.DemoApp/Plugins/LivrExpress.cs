using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : MonoWindowPlugin, IDeliveryService
    {
        readonly ICarRepairService _carRepairService;
        readonly IOutSourcingService _outsourcingService;
        readonly ITimerService _timer;

        public LivrExpress( ICarRepairService carRepairService, IOutSourcingService outsourcingService, ITimerService timer )
            : base( true )
        {
            _carRepairService = carRepairService;
            _outsourcingService = outsourcingService;
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

        public void Repair()
        {
            _carRepairService.Repair();
        }

        public void GetEmployees()
        {
            _outsourcingService.GetEmployees();
        }

        void IDeliveryService.Deliver()
        {
            
        }
    }
}
