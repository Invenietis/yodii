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

        protected override void DoStart()
        {
            _delivery.ServiceStatusChanged += DeliveryServiceStatusChanged;
        }

        protected override void DoStop()
        {
            _delivery.ServiceStatusChanged -= DeliveryServiceStatusChanged;
        }

        void DeliveryServiceStatusChanged( object sender, ServiceStatusChangedEventArgs e )
        {
            if( e.Current == InternalRunningStatus.Stopping )
            {
                if( _reporting.Status == InternalRunningStatus.Started ) 
                {
                    _reporting.Service.Report( "Starting buffering." );
                }
            }
            else if( e.Current == InternalRunningStatus.Started )
            {
                if( this.HasBufferedData )
                {
                    //Debug.Assert( _reporting.CanStart, "Since it is a Runnable dependency." );
                    e.TryStart( _reporting, s => s.Report( "Delivering buffered data!" ) );
                }
            }
        }
    
public  bool HasBufferedData { get; set; }
    }


    class DefaultSoftTimerService : IYodiiPlugin, ISoftTimerService
    {
        DispatcherTimer _base;


        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            _base = new DispatcherTimer( DispatcherPriority.Normal );
            _base.Interval = TimeSpan.FromMilliseconds( 100.0 );
            _base.Tick += _base_Tick;
            return true;
        }

        void _base_Tick( object sender, EventArgs e )
        {
            var h = BaseTimer;
            if( h != null ) h( this, EventArgs.Empty );
        }

        void IYodiiPlugin.Start()
        {
            _base.Start();
        }

        void IYodiiPlugin.Stop()
        {
            _base.Stop();
        }

        void IYodiiPlugin.Teardown()
        {
            _base = null;
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
