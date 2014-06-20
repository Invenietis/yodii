using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface ISecuredDeliveryService : IDeliveryService
    {
        void DeliverSecurely(IProductInfo product, IClientInfo destination);
    }
}
