using System;
using System.Collections.ObjectModel;
using Yodii.Model;
using System.Collections.Generic;

namespace Yodii.DemoApp
{
    public interface IMarketPlaceService : IYodiiService
    {
        bool PlaceOrder( IClientInfo clientInfo, MarketPlace.Product product = null );

        void AddNewProduct( MarketPlace.Product product );

        ObservableCollection<MarketPlace.Product> Products { get; }

        List<IConsumer> Consumers { get; }
    }
}
