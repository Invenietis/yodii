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
        ITimerService _timer;

        public Company2( IRunningService<IMarketPlaceService> marketPlace, IRunningService<IDeliveryService> delivery, ITimerService timer )
            : base( true )
        {
            _marketPlace = marketPlace;
            _delivery = delivery;
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new Company2View()
            {
                DataContext = this
            };
            Window.Show();
            return Window;
        }

        protected override void DestroyWindow()
        {
            if( Window != null ) Window.Close();
        }
    }
}
