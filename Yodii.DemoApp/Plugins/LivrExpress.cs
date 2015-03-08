#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\LivrExpress.cs) is part of CiviKey. 
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
using System.Windows;
using Yodii.DemoApp.Examples.Plugins.Views;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class LivrExpress : MonoWindowPlugin, IDeliveryService
    {
        readonly ICarRepairService _carRepairService;
        readonly IOutSourcingService _outsourcingService;
        readonly IMarketPlaceService _marketPlace;

        public LivrExpress( ICarRepairService carRepairService, IOutSourcingService outsourcingService, IMarketPlaceService market )
            : base( true )
        {
            _carRepairService = carRepairService;
            _outsourcingService = outsourcingService;
            _marketPlace = market;
        }

        protected override Window CreateWindow()
        {
            Window = new LivrExpressView()
            {
                DataContext = this
            };

            return Window;
        }

        public bool Repair()
        {
            return _carRepairService.Repair();
        }

        public bool GetEmployees()
        {
            return _outsourcingService.GetEmployees();
        }

        void IDeliveryService.Deliver( Tuple<IClientInfo, MarketPlace.Product> order )
        {
            IConsumer client = _marketPlace.Consumers.Find( c => c.Info == order.Item1 );
            if( client != null )
            {
                client.ReceiveDelivery( order.Item2 );
            }
        }
    }
}
