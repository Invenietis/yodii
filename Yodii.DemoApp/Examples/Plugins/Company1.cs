using System;
using System.Collections.Generic;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company1 : MonoWindowPlugin
    {
        readonly IService<IMarketPlaceService> _serviceRef1;
        readonly IService<IDeliveryService> _serviceRef2;
        readonly ITimerService _timer;
        List<string> _products;

        public Company1( IRunningService<IMarketPlaceService> ServiceRef1, IRunningService<IDeliveryService> ServiceRef2, ITimerService timer, bool runningLifetimeWindow )
            : base( runningLifetimeWindow )
        {
            _serviceRef1 = ServiceRef1;
            _serviceRef2 = ServiceRef2;
            _timer = timer;
            _products = new List<string>();
        }

        void ReleaseNewProduct( string name )
        {
            if( _products.Contains( name ) ) return;
            _products.Add( name );

            RaiseNewNotification( "New Product: " + name );
        }

        private void RaiseNewNotification( string p )
        {
            throw new NotImplementedException();
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new Company1View()
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
    }
}
