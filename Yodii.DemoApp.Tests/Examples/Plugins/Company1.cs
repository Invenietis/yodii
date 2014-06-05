using System;
using System.Collections.Generic;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _serviceRef1;
        readonly IDeliveryService _serviceRef2;
        readonly ITimerService _timer;
        List<string> _products;

        public Company1( IMarketPlaceService ServiceRef1, IDeliveryService ServiceRef2, ITimerService timer, bool runningLifetimeWindow, Window window )
            : base( runningLifetimeWindow, window )
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

            RaiseNewNotification( "New Product : " + name );
        }

        private void RaiseNewNotification( string p )
        {
            throw new NotImplementedException();
        }

        protected override Window CreateAndShowWindow()
        {
            throw new NotImplementedException();
        }

        protected override void DestroyWindow()
        {
            throw new NotImplementedException();
        }
    }
}
