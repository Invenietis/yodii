using System;
using System.Windows;
using System.Timers;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Timer : MonoWindowPlugin, ITimerService
    {
        readonly ITimerService _timer;

        public Timer( ITimerService timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
            _timer = timer;
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new TimerView()
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
