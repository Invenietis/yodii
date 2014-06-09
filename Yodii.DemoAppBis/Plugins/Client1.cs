using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _market;
        readonly ITimerService _timer;

        public Client1( IMarketPlaceService market, ITimerService timer )
            : base( true )
        {
            _timer = timer;
            _market = market;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new Client1View()
            {
                DataContext = this
            };

            Window.Show();

            return Window;
        }

        protected override void DestroyWindow()
        {
            if( Window != null )
                Window.Close();
        }
    }
}
