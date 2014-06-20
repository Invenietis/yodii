using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        ObservableCollection<Product> _products;
        List<IConsumer> _consumers;
        List<IBusiness> _companies;

        public MarketPlace()
            : base( true ) 
        {
            _products = new ObservableCollection<Product>();
            _consumers = new List<IConsumer>();
            _companies = new List<IBusiness>();
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
            if( !_consumers.Contains( client ) )
            {
                _consumers.Add( client );
            }

            foreach( IBusiness company in _companies )
            {
                company.AddNewClientOrder( client );
            }
        }

       public void AddNewProduct( Product p )
        {
            if( _products.Contains( p ) ) return;
            _products.Add( p );
            RaisePropertyChanged();
        }

        public ObservableCollection<Product> Products
        {
            get { return _products; }
        }

        public List<IConsumer> Consumers
        {
            get { return _consumers; }
        }

        public List<IBusiness> Companies
        {

            get { return _companies; }
        }

        public abstract class Product : NotifyPropertyChangedBase, IProductInfo
        {
            string _name;
            ProductCategory _productCategory;
            int _price;
            DateTime _creationDate;

            public string Producer { get; set; }// juste pour etre conforme à IproductInfo

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

            public DateTime CreationDate
            {
                get
                {
                    return _creationDate;
                }
                set
                {
                    _creationDate = value;
                    RaisePropertyChanged();
                }
            }
        }
    }
}
