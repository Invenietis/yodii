using System;

namespace Yodii.DemoApp
{
    public interface IConsumer
    {
        IClientInfo Info { get; }
    
        bool ReceiveDelivery( MarketPlace.Product purchasedProduct );
    }
}
