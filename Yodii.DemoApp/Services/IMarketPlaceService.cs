using System;
using System.Collections.ObjectModel;
using Yodii.Model;
using System.Collections.Generic;

namespace Yodii.DemoApp
{
    public interface IMarketPlaceService : IYodiiService
    {
        bool CheckNewProducts( IConsumer client );

        bool PlaceOrder( IClientInfo clientInfo, IProductInfo product);

        void AddNewProduct( IProductInfo product /*, IBusiness ? on verra*/);

        ObservableCollection<IProductInfo> Products { get; }
    }
}
