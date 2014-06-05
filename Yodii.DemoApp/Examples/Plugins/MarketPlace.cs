using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        readonly Client1 _client;
        readonly Company1 _company;
        readonly IService<ITimerService> _timer;

        public MarketPlace( Client1 client, Company1 company, IRunningService<ITimerService> timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow ) 
        {
            _client = client;
            _company = company;
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
