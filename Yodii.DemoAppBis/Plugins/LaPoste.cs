using System;
using System.Windows;
using Yodii.Model;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class LaPoste : MonoWindowPlugin, ISecuredDeliveryService
    {
        ITimerService _timer;

        public LaPoste( ITimerService timer )
            : base( true ) 
        {
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new LaPosteView()
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
