using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IConsumer : IYodiiService
    {
        IClientInfo Info { get; }
    
        bool ReceiveDelivery( MarketPlace.Product purchasedProduct );
    }
}
