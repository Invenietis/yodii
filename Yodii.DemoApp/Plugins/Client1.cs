#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\Client1.cs) is part of CiviKey. 
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
using System.Collections.ObjectModel;
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Client1 : MonoWindowPlugin, IConsumer
    {
        readonly IMarketPlaceService _market;
        readonly ClientInfo _clientInfo;
        ObservableCollection<MarketPlace.Product> _purchasedProducts;

        public Client1( IMarketPlaceService market/*, string name, string adress*/ )
            : base( true )
        {
            _market = market;
            //_market.Consumers.Add( this );
            _clientInfo = new ClientInfo( /*name*/"Client1", /*adress*/"aba" );
            _purchasedProducts = new ObservableCollection<MarketPlace.Product>();
        }

        public void Buy( MarketPlace.Product product = null )
        {
            if( product != null && _market.Products.Contains( product ) )
                _market.PlaceOrder( _clientInfo, product );
        }

        public bool ReceiveDelivery( MarketPlace.Product purchasedProduct )
        {
            _purchasedProducts.Add( purchasedProduct );
            RaisePropertyChanged();
            return true;
        }

        public IClientInfo Info { get { return _clientInfo; } }

        public ObservableCollection<MarketPlace.Product> PurchasedProducts { get { return _purchasedProducts; } }

        public ObservableCollection<MarketPlace.Product> AvailableProducts { get { return _market.Products; } }

        protected override Window CreateWindow()
        {
            Window = new Client1View()
            {
                DataContext = this
            };
            _market.Consumers.Add( this );
            return Window;
        }
    }
}
