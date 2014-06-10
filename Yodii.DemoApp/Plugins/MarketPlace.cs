using System;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        readonly ITimerService _timer;
        ObservableCollection<string> _products;

        public MarketPlace( ITimerService timer )
            : base( true ) 
        {
            _timer = timer;
            _products = new ObservableCollection<string>();
        }

        protected override Window CreateAndShowWindow()
        {
            Window = new MarketPlaceView()
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

        void IMarketPlaceService.CheckNewProducts()
        {
            throw new NotImplementedException();
        }

        void IMarketPlaceService.AddNewProducts( string name )
        {
            if( _products.Contains( name ) ) return;
            _products.Add( name );
            RaiseNewNotification( "product added: " + name );
        }

        private void RaiseNewNotification( string p )
        {
            throw new NotImplementedException();
        }

        public ObservableCollection<string> Products
        {
            get { return _products; }
        }
    }
}
