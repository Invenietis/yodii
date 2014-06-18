using System;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        ObservableCollection<string> _products;
        
        public MarketPlace()
            : base( true ) 
        {
            _products = new ObservableCollection<string>();
        }

        protected override Window CreateWindow()
        {
            Window = new MarketPlaceView()
            {
                DataContext = this
            };

            return Window;
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

        public abstract class Product
        {
            public string Name;
        }
    }
}
