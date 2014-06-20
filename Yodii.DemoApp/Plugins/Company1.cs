using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class Company1 : MonoWindowPlugin, IBusiness
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _deliveryService;
        ObservableCollection<ProductCompany1> _products;
        ObservableCollection<Tuple<Guid, IConsumer>> _orders;

        public Company1( IMarketPlaceService marketPlace, IDeliveryService deliveryService )
            : base( true )
        {
            _marketPlace = marketPlace;
            _deliveryService = deliveryService;
            _products = new ObservableCollection<ProductCompany1>();
            _orders = new ObservableCollection<Tuple<Guid,IConsumer>>();
        }


        public void AddNewProduct( string name, ProductCategory category, int price )
        {
            Debug.Assert( !string.IsNullOrEmpty( name ) );
            Debug.Assert( price > 0 );

            ProductCompany1 p = new ProductCompany1( name, category, price, DateTime.Now );
            _marketPlace.AddNewProduct( p );
            _products.Add( p );
            //RaiseNewNotification( "New Product: " + name );
        }

        public void AddNewClientOrder( IConsumer client )
        {
        }

        public void AddNewDeliveryOrder()
        {
        }

        public ObservableCollection<ProductCompany1> Products
        {
            get { return _products; }
        }

        protected override Window CreateWindow()
        {
            Window = new Company1View()
            {
                DataContext = this
            };

            return Window;
        }

        public class ProductCompany1 : Yodii.DemoApp.MarketPlace.Product
        {
            public ProductCompany1( string name, ProductCategory category, int price, DateTime creationDate )
            {
                Name = name;
                ProductCategory = category;
                Price = price;
                CreationDate = creationDate;
            }
        }
    }
}
