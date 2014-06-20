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

        public LivrExpress( ICarRepairService carRepairService, IOutSourcingService outsourcingService )
            : base( true )
        {
            _carRepairService = carRepairService;
            _outsourcingService = outsourcingService;
        }

        protected override Window CreateWindow()
        {
            Window = new LivrExpressView()
            {
                DataContext = this
            };

            return Window;
        }

        public bool Repair()
        {
            return _carRepairService.Repair();
        }

        public bool GetEmployees()
        {
            return _outsourcingService.GetEmployees();
        }

        void IDeliveryService.Deliver( IProductInfo product, IClientInfo client )
        {
        }
    }
}
