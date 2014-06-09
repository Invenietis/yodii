using System;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Garage : MonoWindowPlugin, ICarRepairService
    {
        ITimerService _timer;

        public Garage(  ITimerService timer )
            : base( true )
        {
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new GarageView()
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
