using System;
using System.Collections.Generic;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Company1 : MonoWindowPlugin
    {
        readonly IMarketPlaceService _marketPlace;
        readonly IDeliveryService _deliveryService;
        List<ProductCompany1> _products;

        public Company1( IMarketPlaceService marketPlace, IDeliveryService deliveryService )
            : base( true )
        {
            _marketPlace = marketPlace;
            _deliveryService = deliveryService;
            _products = new List<ProductCompany1>();
        }

        void AddNewProduct( string name )
        {
            _marketPlace.AddNewProducts( name );
            RaiseNewNotification( "New Product: " + name );
        }

        private void RaiseNewNotification( string p )
        {
            throw new NotImplementedException();
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
            public ProductCompany1()
            {
                Name = "ProductCompany1";
            }
        }
    }
}
