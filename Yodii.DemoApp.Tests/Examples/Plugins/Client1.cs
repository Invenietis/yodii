using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin
    {
        IMarketPlaceService _service;
        ITimerService _timer;

        public Client1( bool runningLifetimeWindow, IMarketPlaceService service, ITimerService timer )
            : base( runningLifetimeWindow )
        {
            _timer = timer;
            _service = service;
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
