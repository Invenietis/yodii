using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company3 : MonoWindowPlugin, IBusiness
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _delivery;
        ObservableCollection<ProductCompany3> _products;

        public Company3( IMarketPlaceService marketPlace, IDeliveryService delivery )
            : base( true )
        {
            _marketPlace = marketPlace;
            _delivery = delivery;
            _products = new ObservableCollection<ProductCompany3>();
        }

        protected override Window CreateWindow()
        {
            Window = new Company2View()
            {
                DataContext = this
            };
            return Window;
        }

        public class ProductCompany3 : Yodii.DemoApp.MarketPlace.Product
        {
            public ProductCompany3( string name, ProductCategory category, int price, DateTime creationDate )
            {
                Name = name;
                ProductCategory = category;
                Price = price;
                CreationDate = creationDate;
            }
        }

        public void AddNewProduct( string name, ProductCategory category, int price )
        {
            Debug.Assert( !string.IsNullOrEmpty( name ) );
            Debug.Assert( price > 0 );

            ProductCompany3 p = new ProductCompany3( name, category, price, DateTime.Now );
            _marketPlace.AddNewProduct( p );
            _products.Add( p );
        }

        public void AddNewClientOrder( IConsumer client )
        {
        }

        public void AddNewDeliveryOrder()
        {

        }
    }
}
