#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\Views\Client1View.xaml.cs) is part of CiviKey. 
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

using System.Windows;

namespace Yodii.DemoApp.Examples.Plugins.Views
{
    /// <summary>
    /// Interaction logic for Client1.xaml
    /// </summary>
    public partial class Client1View : Window
    {
        public Client1View()
        {
            InitializeComponent();

            WindowStartupLocation = WindowStartupLocation.Manual;
            Top = 0;
            Left = 0;
            MinHeight = MinWidth = MaxHeight = MaxWidth = 550;
        }

        private void Buy_Button_Click( object sender, RoutedEventArgs e )
        {
            MarketPlace.Product p = ( MarketPlace.Product )ProductGrid.SelectedItem;
            ( (Client1)DataContext ).Buy( p );
        }
    }
}
