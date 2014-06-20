using System;
using System.Collections.ObjectModel;

namespace Yodii.DemoApp
{
    public interface IBusiness
    {
        void AddNewClientOrder( IConsumer client );//je serais pour l'enlever au profit de celui du dessous, à voir.

        bool NewOrder( IClientInfo clientInfo, IProductInfo product);

        void AddNewDeliveryOrder();            
    }
}
