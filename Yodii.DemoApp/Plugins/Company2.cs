using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company2 : MonoWindowPlugin, IBusiness
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _delivery;
        ObservableCollection<ProductCompany2> _products;
        ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> _orders;

        public Company2( IMarketPlaceService marketPlace, IDeliveryService delivery, IYodiiEngine engine )
            : base( true, engine )
        {
            _marketPlace = marketPlace;
            _delivery = delivery;
            _products = new ObservableCollection<ProductCompany2>();
            _orders = new ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>>();
        }

        protected override Window CreateWindow()
        {
            Window = new Company2View( this )
            {
                DataContext = this
            };
            return Window;
        }

        public class ProductCompany2 : Yodii.DemoApp.MarketPlace.Product
        {
            public ProductCompany2( string name, ProductCategory category, int price, DateTime creationDate, Company2 company )
            {
                Name = name;
                ProductCategory = category;
                Price = price;
                CreationDate = creationDate;
                Company = company;
            }
        }

        internal void AddNewProduct( string name, ProductCategory category, int price )
        {
            Debug.Assert( !string.IsNullOrEmpty( name ) );
            Debug.Assert( price > 0 );

            ProductCompany2 p = new ProductCompany2( name, category, price, DateTime.Now, this );
            _marketPlace.AddNewProduct( p );
            _products.Add( p );
        }

        public bool NewOrder( IClientInfo clientInfo, MarketPlace.Product product )
        {
            Tuple<IClientInfo, MarketPlace.Product> order = new Tuple<IClientInfo, MarketPlace.Product>( clientInfo, product );
            _orders.Add( order );
            RaisePropertyChanged( "newOrder" );
            if( _orders.Count >= 3 )
                HandleOrders();
            return true;
        }

        public void HandleOrders()
        {
            foreach( Tuple<IClientInfo, MarketPlace.Product> order in _orders )
            {
                _delivery.Deliver( order );
            }
            _orders.Clear();          
        }

        public ObservableCollection<Tuple<IClientInfo, MarketPlace.Product>> Orders { get { return _orders; } }

        public ObservableCollection<ProductCompany2> Products { get { return _products; } }
    }
}
