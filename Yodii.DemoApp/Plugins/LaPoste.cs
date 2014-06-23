using System;
using System.Windows;
using Yodii.Model;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class LaPoste : MonoWindowPlugin, ISecuredDeliveryService
    {
        readonly IMarketPlaceService _marketPlace;

        public LaPoste( IMarketPlaceService market )
            : base( true ) 
        {
            _marketPlace = market;
        }

        protected override Window CreateWindow()
        {
            Window = new LaPosteView()
            {
                DataContext = this
            };
            return Window;
        }

        void ISecuredDeliveryService.DeliverSecurely( Tuple<IClientInfo, MarketPlace.Product> order )
        {
            IConsumer client = _marketPlace.Consumers.Find( c => c.Info == order.Item1 );
            if( client != null )
            {
                client.ReceiveDelivery( order.Item2 );
            }
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
