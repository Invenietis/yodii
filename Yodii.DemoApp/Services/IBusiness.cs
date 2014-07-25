using System;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface IBusiness : IYodiiService
    {
        bool NewOrder( IClientInfo clientInfo, MarketPlace.Product product = null );
    }
}
