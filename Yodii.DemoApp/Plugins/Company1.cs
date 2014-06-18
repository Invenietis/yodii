using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class Company1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _deliveryService;
        ObservableCollection<ProductCompany1> _products;

        public Company1( IMarketPlaceService marketPlace, IDeliveryService deliveryService )
            : base( true )
        {
            _marketPlace = marketPlace;
            _deliveryService = deliveryService;
            _products = new ObservableCollection<ProductCompany1>();
        }

        public void AddNewProduct( string name, ProductCategory category, int price )
        {
            Debug.Assert( !string.IsNullOrEmpty( name ) );
            Debug.Assert( price != null && price > 0 );
            ProductCompany1 p = new ProductCompany1( name, category, price );
            //_marketPlace.AddNewProducts( name, category, price );
            _products.Add( p );
            //RaiseNewNotification( "New Product: " + name );
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
            public ProductCompany1( string name, ProductCategory category, int price )
            {
                Name = name;
                ProductCategory = category;
                Price = price;
            }
        }
    }
}
