using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class UPS : MonoWindowPlugin, ISecuredDeliveryService
    {
        readonly ITimerService _timer;

        public UPS( ITimerService timer )
            : base( true )
        {
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new UPSView()
            {
                DataContext = this
            };

            Window.Show();
            return Window;
        }

        protected override void DestroyWindow()
        {
            throw new NotImplementedException();
        }
    }
}
