using System;
using System.Windows;
using System.Timers;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Timer : MonoWindowPlugin, ITimerService
    {
        public Timer()
            : base( true )
        {
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

        public void StopTimer()
        {
            throw new NotImplementedException();
        }

        public void StartTimer()
        {
            throw new NotImplementedException();
        }
    }
}
