using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LaPoste : MonoWindowPlugin, ISecuredDeliveryService
    {
        ITimerService _timer;

        public LaPoste( bool runningLifetimeWindow, ITimerService timer )
            : base( runningLifetimeWindow ) 
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
