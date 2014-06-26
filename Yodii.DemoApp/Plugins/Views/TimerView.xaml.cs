using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Timers;

namespace Yodii.DemoApp.Examples.Plugins.Views
{
    /// <summary>
    /// Interaction logic for Client1.xaml
    /// </summary>
    public partial class TimerView : Window
    {
        TimerHandler _timer;
        TimeSpan _defaultIntervalValue;
        public TimerView(TimerHandler timer)
        {
            _timer = timer;
            _defaultIntervalValue = new TimeSpan(_timer.Timer.Interval.Milliseconds);
            _timer.Timer.Tick += heartbeat;
            _timer.Timer.Start();

            InitializeComponent();

            //Start up top right corner.
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = SystemParameters.PrimaryScreenWidth - this.Width;
        }


       /* public void SliderChanged( object sender, RoutedPropertyChangedEventArgs<double> e )
        {
            if (e.NewValue!=0)
              _timer.SetSpeed( e.NewValue );
            if (intervalValue!=null)
                intervalValue.Content = e.NewValue.ToString();
        }

        private void intervalvalueInitialized( object sender, EventArgs e )
        {
            intervalValue.Content = _timer.Timer.Interval.ToString();
        }

        private void sliderInitialized( object sender, EventArgs e )
        {
            slider.Value = _defaultIntervalValue;
        }*/

        private void heartbeat(object sender, EventArgs a)
        {
            if( heart.Visibility != System.Windows.Visibility.Visible )
                heart.Visibility = System.Windows.Visibility.Visible;
            else
                heart.Visibility = System.Windows.Visibility.Hidden;
        }
    }
}
