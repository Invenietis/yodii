using System;
using System.Collections.ObjectModel;

namespace Yodii.DemoApp
{
    public interface IBusiness
    {
        void AddNewProduct( string name, ProductCategory category, int price );

        void AddNewClientOrder( IConsumer client );

        void AddNewDeliveryOrder();            
    }
}
