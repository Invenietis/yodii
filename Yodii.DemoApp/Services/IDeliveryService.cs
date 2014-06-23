using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IDeliveryService : IYodiiService
    {
        void Deliver( Tuple<IClientInfo, MarketPlace.Product> order );
    }
}
