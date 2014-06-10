using System;
using System.Collections.Generic;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _deliveryService;
        readonly ITimerService _timer;

        public Company1( IMarketPlaceService marketPlace, IDeliveryService deliveryService, ITimerService timer )
            : base( true )
        {
            _marketPlace = marketPlace;
            _deliveryService = deliveryService;
            _timer = timer;
        }

        void AddNewProduct( string name )
        {
            _marketPlace.AddNewProducts( name );
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
