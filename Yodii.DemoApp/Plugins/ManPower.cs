using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class ManPower : MonoWindowPlugin, IOutSourcingService
    {
        readonly ITimerService _timer;

        public ManPower(  ITimerService timer )
            : base( true )
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

        public void GetEmployees()
        {

        }
    }
}
