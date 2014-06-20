using System;
using System.Collections.ObjectModel;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IMarketPlaceService : IYodiiService
    {
        void CheckNewProducts( IConsumer client );

        void AddNewProduct( MarketPlace.Product product );

        ObservableCollection<MarketPlace.Product> Products { get; }
    }
}
