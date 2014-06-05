using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LaPoste : MonoWindowPlugin, ISecuredDeliveryService
    {
        ITimerService _timer;

        public LaPoste( ITimerService timer, bool runningLifetimeWindow, Window window )
            : base( runningLifetimeWindow, window ) 
        {
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            throw new NotImplementedException();
        }

        protected override void DestroyWindow()
        {
            throw new NotImplementedException();
        }
    }
}
