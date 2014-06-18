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

        public void CheckNewProducts( IConsumer client )
        {
            //Check in local market list
            //AND/OR
            //Ask companies for new products
            //Get a product
            //Throw it to delivery services
        }
        void IMarketPlaceService.AddNewProducts( string name )
        {
            if( _products.Contains( name ) ) return;
            _products.Add( name );
            RaisePropertyChanged();
        }

        public ObservableCollection<string> Products
        {
            get { return _products; }
        }

        public abstract class Product : NotifyPropertyChangedBase
        {
            string _name;
            ProductCategory _productCategory;
            int _price;

            public string Name
            {
                get
                {
                    return _name;
                }
                set
                {
                    _name = value;
                    RaisePropertyChanged();
                }
            }

            public ProductCategory ProductCategory 
            {
                get 
                {
                    return _productCategory;
                }
                set
                {
                    _productCategory = value;
                    RaisePropertyChanged();
                }
            }

            public int Price
            {
                get
                {
                    return _price;
                }
                set
                {
                    _price = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
