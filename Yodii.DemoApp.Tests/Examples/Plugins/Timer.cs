using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Timer : MonoWindowPlugin, ITimerService
    {
        public Timer( bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
        }

        protected override Window CreateAndShowWindow()
        {
            throw new NotImplementedException();
        }

        protected override void DestroyWindow()
        {
            throw new NotImplementedException();
        }

        public void IncreaseSpeed()
        {
            throw new NotImplementedException();
        }

        public void DecreaseSpeed()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }
    }
}
