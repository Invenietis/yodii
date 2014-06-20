using System;

namespace Yodii.DemoApp
{
    public interface IConsumer
    {
        void Buy();

        string Name { get; }

        string Adress { get; }

        void ReceiveDelivery( MarketPlace.Product purchasedProduct );
    }
}
