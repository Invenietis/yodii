using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IMarketPlaceService : IYodiiService
    {
        void CheckNewProducts();

        void AddNewProducts( string name );
    }
}
