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
        ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> _orders;

        public Company3( IMarketPlaceService marketPlace, IDeliveryService delivery, IYodiiEngine engine )
            : base( true, engine )
        {
            _marketPlace = marketPlace;
            _delivery = delivery;
            _products = new ObservableCollection<ProductCompany3>();
            _orders = new ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>>();
        }

        protected override Window CreateWindow()
        {
            Window = new Company3View( this )
            {
                DataContext = this
            };
            return Window;
        }

        public class ProductCompany3 : Yodii.DemoApp.MarketPlace.Product
        {
            public ProductCompany3( string name, ProductCategory category, int price, DateTime creationDate, Company3 company )
            {
                Name = name;
                ProductCategory = category;
                Price = price;
                CreationDate = creationDate;
                Company = company;
            }
        }

        public void AddNewProduct( string name, ProductCategory category, int price )
        {
            Debug.Assert( !string.IsNullOrEmpty( name ) );
            Debug.Assert( price > 0 );

            ProductCompany3 p = new ProductCompany3( name, category, price, DateTime.Now, this );
            _marketPlace.AddNewProduct( p );
            _products.Add( p );
        }

        public bool NewOrder( IClientInfo clientInfo, MarketPlace.Product product )
        {
            Tuple<IClientInfo, MarketPlace.Product> order = new Tuple<IClientInfo, MarketPlace.Product>( clientInfo, product );
            if( _orders.Contains( order ) ) return false;
            _orders.Add( order );
            RaisePropertyChanged( "newOrder" );
            HandleOrders();
            return true;
        }

        public void HandleOrders()
        {
            if( _orders.Count == 3 )
            {
                foreach( Tuple<IClientInfo, MarketPlace.Product> order in _orders )
                {
                    _delivery.Deliver( order );
                }
                _orders.Clear();
            }
        }

        ObservableCollection<ProductCompany3> Products { get { return _products; } }

        ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> Orders { get { return _orders; } }
    }
}
