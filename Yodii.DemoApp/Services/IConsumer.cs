using System;

namespace Yodii.DemoApp
{
    public interface IConsumer
    {
        IClientInfo Info { get; }
    
        void ReceiveDelivery( MarketPlace.Product purchasedProduct );
    }
}
