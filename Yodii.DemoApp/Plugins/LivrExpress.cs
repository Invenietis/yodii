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
        readonly IMarketPlaceService _marketPlace;

        public LivrExpress( ICarRepairService carRepairService, IOutSourcingService outsourcingService, IMarketPlaceService market )
            : base( true )
        {
            _carRepairService = carRepairService;
            _outsourcingService = outsourcingService;
            _marketPlace = market;
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

        void IDeliveryService.Deliver( Tuple<IClientInfo, MarketPlace.Product> order )
        {
            IConsumer client = _marketPlace.Consumers.Find( c => c.Info == order.Item1 );
            if( client != null )
            {
                client.ReceiveDelivery( order.Item2 );
            }
        }
    }
}
