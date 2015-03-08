#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\ManPower.cs) is part of CiviKey. 
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
    public class ManPower : MonoWindowPlugin, IOutSourcingService
    {
        public ManPower()
            : base( true )
        {
            _nbEmployees = _maxAvailable;
        }
        const int _maxAvailable=20;
        int _nbEmployees;
        public int NBemployees { get { return _nbEmployees; } set { _nbEmployees = value; RaisePropertyChanged(); } }
        protected override Window CreateWindow()
        {
            Window = new ManPowerView()
            {
                DataContext = this
            };
            return Window;
        }

        public bool GetEmployees()
        {
            NBemployees--;
            return true;
        }
        public void  ReturnEmployees(int nbReturned)
        {
            NBemployees += nbReturned;
        }
    }
}
