using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IMarketPlaceService : IYodiiService
    {
        void CheckNewProducts( IConsumer client );

        void AddNewProducts( string name );
    }
}
