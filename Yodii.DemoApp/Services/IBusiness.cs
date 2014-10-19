using System;

namespace Yodii.DemoApp
{
    public interface IBusiness
    {
        bool NewOrder( IClientInfo clientInfo, MarketPlace.Product product = null );
    }
}
