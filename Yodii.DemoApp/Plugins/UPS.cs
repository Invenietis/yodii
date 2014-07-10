using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Yodii.DemoApp
{
    public class UPS : MonoWindowPlugin, ISecuredDeliveryService
    {
        IOptionalService<IConsumer> _client;
        readonly IMarketPlaceService _marketPlace;
        ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> _delivered;
        public ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> Delivered { get { return _delivered; } }

        public UPS( IMarketPlaceService market, IOptionalService<IConsumer> client, IYodiiEngine engine )
            : base( true, engine )
        {
            _client = client;
            _marketPlace = market;
            _delivered = new ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>>();
        }

        protected override Window CreateWindow()
        {
            Window = new UPSView( this )
            {
                DataContext = this
            };

            return Window;
        }

        void IDeliveryService.Deliver( Tuple<IClientInfo, MarketPlace.Product> order )
        {
            IConsumer client = _marketPlace.Consumers.Find( c => c.Info == order.Item1 );
            if( client != null && _client.Status == InternalRunningStatus.Started) 
            {
                client.ReceiveDelivery( order.Item2 );
                _delivered.Add( order );
            }
        }
    }
}
