#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\MarketPlace.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;

namespace Yodii.DemoApp
{
    public class MarketPlace : MonoWindowPlugin, IMarketPlaceService
    {
        ObservableCollection<MarketPlace.Product> _products;
        List<IConsumer> _consumers;
        List<IBusiness> _companies;

        public MarketPlace()
            : base( true ) 
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
