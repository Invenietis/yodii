using System;
using System.Timers;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class TimerHandler : MonoWindowPlugin, ITimerService
    {
        readonly Timer _timer;

        public TimerHandler()
            : base( true )
        {
            _timer = new Timer();
        }

        protected override Window CreateWindow()
        {
            Window = new TimerView()
            {
                DataContext = this
            };

            return Window;
        }

        void ITimerService.IncreaseSpeed()
        {
            if( _timer.Interval >= 6 )
                _timer.Interval -= 5;
        }

        void ITimerService.DecreaseSpeed()
        {
            _timer.Interval += 5;
        }

        void ITimerService.Stop()
        {
            _timer.Stop();
        }

        void ITimerService.Start()
        {
            _timer.Start();    
        }

        public Timer Timer
        {
            get { return _timer; }
        }
    }
}
