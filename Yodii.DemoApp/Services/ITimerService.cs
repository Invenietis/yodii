#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Services\ITimerService.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Windows.Threading;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public interface ITimerService : IYodiiService
    {
        void IncreaseSpeed();

        void DecreaseSpeed();

        void Stop();

        void Start();

        void SubscribeToTimerEvent( Action<object, EventArgs> methodToAdd );
        void UnsubscribeToTimerEvent( Action<object, EventArgs> methodToRemove );
    }

    /// <summary>
    /// A soft timer is actually managed by <see cref="ISoftTimerService"/>.
    /// </summary>
public interface ISoftTimer : IDisposable
{
    /// <summary>
    /// Actual time depends on this <see cref="Interval"/> and <see cref="ISoftTimerService.CurrentRatio"/> values.
    /// Use <see cref="Enabled"/> to temporarily suspend it.
    /// </summary>
    event EventHandler Fire;

    /// <summary>
    /// Gets or sets the theoretical timer time.
    /// </summary>
    TimeSpan Interval { get; set; }

    /// <summary>
    /// Gets or sets whether this timer is actually working.
    /// </summary>
    bool Enabled { get; set; }
}

/// <summary>
/// Centralizes timers so that the actual timers frequencies
/// can be globally adjusted.
/// </summary>
public interface ISoftTimerService : IYodiiService
{
    /// <summary>
    /// This timer fires 10 times a second when <see cref="CurrentRatio"/> is 1.0.
    /// </summary>
    event EventHandler BaseTimer;

    /// <summary>
    /// Gets the current ration that applies to all
    /// existing <see cref="SoftTimer"/>.
    /// Defaults to 1.0.
    /// </summary>
    double CurrentRatio { get; }

    /// <summary>
    /// Creates a new <see cref="SoftTimer"/>.
    /// </summary>
    /// <param name="interval">Initial <see cref="ISoftTimer.Interval"/>.</param>
    /// <param name="enabled">False to create a disabled timer.</param>
    /// <returns>A new timer that should be disposed once useless.</returns>
    ISoftTimer Create( TimeSpan interval, bool enabled = true );
}



    public interface IReportingService : IYodiiService
    {
        void Report(string s );
    }

    public class SamplePlugin : YodiiPluginBase
    {
        readonly ISoftTimerService _timer;
        readonly IOptionalService<IDeliveryService> _delivery;
        readonly IRunnableRecommendedService<IReportingService> _reporting;

        public SamplePlugin( ISoftTimerService timer, 
                                IOptionalService<IDeliveryService> delivery, 
                                IRunnableRecommendedService<IReportingService> reporting )
        {
            _timer = timer;
            _delivery = delivery;
            _reporting = reporting;
        }

        protected override void PluginStart( IStartContext c )
        {
            _delivery.ServiceStatusChanged += DeliveryServiceStatusChanged;
            base.PluginStart( c );
        }

        protected override void PluginStop( IStopContext c )
        {
            _delivery.ServiceStatusChanged -= DeliveryServiceStatusChanged;
            base.PluginStop( c );
        }

        void DeliveryServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( _delivery.Status == ServiceStatus.Stopping )
            {
                if( _reporting.Status == ServiceStatus.Started ) 
                {
                    _reporting.Service.Report( "Starting buffering." );
                }
            }
            else if( _delivery.Status == ServiceStatus.Started )
            {
                if( this.HasBufferedData )
                {
                    e.TryStart( _reporting, () => _reporting.Service.Report( "Delivering buffered data!" ) );
                }
            }
        }
    
public  bool HasBufferedData { get; set; }
    }


    class DefaultSoftTimerService : YodiiPluginBase, ISoftTimerService
    {
        DispatcherTimer _base;

        protected override void PluginPreStart( IPreStartContext c )
        {
            _base = new DispatcherTimer( DispatcherPriority.Normal );
            _base.Interval = TimeSpan.FromMilliseconds( 100.0 );
            _base.Tick += _base_Tick;
            base.PluginPreStart( c );
        }

        void _base_Tick( object sender, EventArgs e )
        {
            var h = BaseTimer;
            if( h != null ) h( this, EventArgs.Empty );
        }

        protected override void PluginStart( IStartContext c )
        {
            _base.Start();
            base.PluginStart( c );
        }


        protected override void PluginPreStop( IPreStopContext c )
        {
            _base.Stop();
            base.PluginPreStop( c );
        }

        protected override void PluginStop( IStopContext c )
        {
            _base = null;
            base.PluginStop( c );
        }

        public event EventHandler BaseTimer;

        public double CurrentRatio
        {
            get { return 1.0; }
        }

        public ISoftTimer Create( TimeSpan interval, bool enabled = true )
        {
            throw new NotImplementedException();
        }

        class SimpleTimer : ISoftTimer
        {
            public event EventHandler Fire;

            public TimeSpan Interval
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public bool Enabled
            {
                get
                {
                    throw new NotImplementedException();
                }
                set
                {
                    throw new NotImplementedException();
                }
            }

            public void Dispose()
            {

            }
        }
    }
}
