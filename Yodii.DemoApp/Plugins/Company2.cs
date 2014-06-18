using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : MonoWindowPlugin
    {
        IService<IMarketPlaceService> _marketPlace;
        IService<IDeliveryService> _delivery;

        public Company2( IRunningService<IMarketPlaceService> marketPlace, IRunningService<IDeliveryService> delivery )
            : base( true )
        {
            _marketPlace = marketPlace;
            _delivery = delivery;
        }

        protected override Window CreateWindow()
        {
            Window = new Company2View()
            {
                DataContext = this
            };
            return Window;
        }
    }
}
