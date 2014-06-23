using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface ISecuredDeliveryService : IDeliveryService
    {
        void DeliverSecurely( Tuple<IClientInfo, MarketPlace.Product> order );
    }
}
