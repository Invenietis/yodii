#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.DemoApp\Plugins\Views\AddProductWindow.xaml.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace Yodii.DemoApp.Plugins.Views
{
    /// <summary>
    /// Interaction logic for AddProductWindow.xaml
    /// </summary>
    public partial class AddProductWindow : Window
    {
        public AddProductWindow()
        {
            InitializeComponent();
            ProductCategoryComboBox.ItemsSource = Enum.GetValues( typeof( ProductCategory ) );
            ProductName.Focus(); 
        }

        private void Button_Click_OK( object sender, RoutedEventArgs e )
        {   
            if( string.IsNullOrEmpty( ProductName.Text ) || CheckIfProductAlreadyExists( ProductName.Text ) )
            {
                MessageBox.Show( "Please enter a valid name for this product." );
            }
            else if( ProductCategoryComboBox.SelectedItem == null )
            {
                MessageBox.Show( "Please select a valid category for this product." );
            }
            else if( String.IsNullOrWhiteSpace( ProductPrice.Text ) || int.Parse( ProductPrice.Text ) < 1 )
            {
                MessageBox.Show( "Please enter a valid price for this product." );
            }
            else
            {
                ( (Company1)DataContext ).AddNewProduct( ProductName.Text, (ProductCategory)ProductCategoryComboBox.SelectedValue, int.Parse( ProductPrice.Text ) );
                Close();
            }
        }

        private void NumberValidationTextBox( object sender, TextCompositionEventArgs e )
        {
            Regex r = new Regex( "[^0-9]+" );
            e.Handled = r.IsMatch( e.Text );
        }

        private bool CheckIfProductAlreadyExists( string productName )
        {
            return ( (Company1)DataContext ).MarketPlace.Products.Any( p => p.Name == productName );
        }

        private void Button_Click_Cancel( object sender, RoutedEventArgs e )
        {
            Close();
        }
    }
}
