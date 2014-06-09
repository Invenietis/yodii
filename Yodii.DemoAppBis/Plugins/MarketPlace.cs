using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        readonly ITimerService _timer;

        public MarketPlace( ITimerService timer )
            : base( true ) 
        {
            _timer = timer;
        }

        public void CheckNewProducts()
        {
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new MarketPlaceView()
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
