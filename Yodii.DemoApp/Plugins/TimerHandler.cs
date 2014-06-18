using System;
using System.Timers;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;
using System.Windows.Threading;

namespace Yodii.DemoApp
{
    public class TimerHandler : MonoWindowPlugin, ITimerService
    {
        readonly DispatcherTimer _timer;

        public TimerHandler()
            : base( true )
        {
            _timer = new DispatcherTimer();
            _timer.Interval = new TimeSpan( 0, 0, 0, 0, 1 );

        }

        protected override Window CreateWindow()
        {
            Window = new TimerView( this )
            {
                DataContext = this
            };

            return Window;
        }

        void ITimerService.IncreaseSpeed()
        {
            if( _timer.Interval.Seconds >= 6 )
                _timer.Interval.Subtract( new TimeSpan( 5 ) );
        }

        void ITimerService.DecreaseSpeed()
        {
            _timer.Interval.Add( new TimeSpan( 5 ) );
        }
        internal void SetSpeed( double interval )
        {
            _timer.Interval = new TimeSpan( (long)interval );
        }
        void ITimerService.Stop()
        {
            _timer.Stop();
            if( Window != null ) Window.Close();
        }

        void ITimerService.Start()
        {
            _timer.Start();
            Window = new TimerView( this )
            {
                DataContext = this
            };
            Window.Show();
        }

        public DispatcherTimer Timer
        {
            get { return _timer; }
        }

        public double Interval
        {
            get
            {
                return _timer.Interval.TotalMilliseconds;
            }
            set
            {
                _timer.Stop();
                _timer.Interval = new TimeSpan( 0, 0, 0, 0, Convert.ToInt32(value) );
                _timer.Start();
                RaisePropertyChanged();
            }
        }
    }
}
