#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\Views\TimerView.xaml.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

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

            //Binding myBinding = new Binding( "StartStop" );

            //myBinding.Source = timer;
            //ButtonPause.SetBinding( Button.ContentProperty, myBinding );
            //Start up top right corner.
            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = SystemParameters.PrimaryScreenWidth - this.Width;

        }


        private void heartbeat(object sender, EventArgs a)
        {
            if( heart.Visibility != System.Windows.Visibility.Visible )
                heart.Visibility = System.Windows.Visibility.Visible;
            else
                heart.Visibility = System.Windows.Visibility.Hidden;
        }

        private void Button_Click( object sender, RoutedEventArgs e )
        {
                if(  _timer.Timer.IsEnabled )
                {
                        _timer.Timer.Stop();
                        _timer.StartStop = "";
                }
                  else
                {
                        _timer.Timer.Start();
                        _timer.StartStop = "";
                }
        }

    }
}
