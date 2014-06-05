using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin
    {
        readonly IService<IMarketPlaceService> _market;
        readonly ITimerService _timer;

        public Client1( IRunningService<IMarketPlaceService> market, ITimerService timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
            _timer = timer;
            _market = market;
        }

        //void BuyNewProduct()
        //{
        //    _market.CheckNewProducts();
        //}

        protected override Window CreateAndShowWindow()
        {
            Window = new Client1View()
            {
                DataContext = this
            };

            Window.Show();
            RaiseNewNotification();

            return Window;
        }

        private void RaiseNewNotification()
        {
            throw new NotImplementedException();
        }

        protected override void DestroyWindow()
        {
            if( Window != null )
                Window.Close();
        }
    }
}
