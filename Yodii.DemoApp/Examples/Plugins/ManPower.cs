using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class ManPower : MonoWindowPlugin, IOutSourcingService
    {
        readonly ITimerService _timer;

        public ManPower(  ITimerService timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new ManPowerView()
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
