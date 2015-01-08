#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\TimerHandler.cs) is part of CiviKey. 
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
        public void SubscribeToTimerEvent( Action<object, EventArgs> methodToAdd )
        {           
            EventHandler handler = new EventHandler(methodToAdd);
            _timer.Tick += handler;
            
        }
        public void UnsubscribeToTimerEvent( Action<object, EventArgs> methodToRemove )
        {
            EventHandler handler = new EventHandler( methodToRemove );
            _timer.Tick -= handler;
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
            StartStop = "0";
        }

        void ITimerService.Start()
        {
            _timer.Start();
            Window = new TimerView( this )
            {
                DataContext = this
            };
            Window.Show();
            StartStop = "";
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
                StartStop = "";
                RaisePropertyChanged();
            }
        }
        public string StartStop
        {
            get
            {
                return _timer.IsEnabled ? "Stop" : "Start";

            }
            set
            {
                RaisePropertyChanged();
            }
        }
    }
}
