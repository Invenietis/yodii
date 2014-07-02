using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        ObservableCollection<MarketPlace.Product> _products;
        List<IConsumer> _consumers;
        List<IBusiness> _companies;

        public MarketPlace( IYodiiEngine engine )
            : base( true, engine )
        {
            _products = new ObservableCollection<MarketPlace.Product>();
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

        public void AddNewProduct( MarketPlace.Product p )
        {
            if( _products.Contains( p ) ) return;
            _products.Add( p );
            RaisePropertyChanged();
        }

        public ObservableCollection<MarketPlace.Product> Products
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

        public abstract class Product : NotifyPropertyChangedBase
        {
            string _name;
            ProductCategory _productCategory;
            int _price;
            DateTime _creationDate;
            IBusiness _company;

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

            public IBusiness Company
            {
                get
                {
                    return _company;
                }
                set
                {
                    _company = value;
                    RaisePropertyChanged();
                }
            }

        }

        public bool PlaceOrder( IClientInfo clientInfo, MarketPlace.Product productInfo = null )
        {
            return productInfo.Company.NewOrder( clientInfo, productInfo );
        }

        ObservableCollection<MarketPlace.Product> IMarketPlaceService.Products
        {
            get { return _products; }
        }
    }
}
