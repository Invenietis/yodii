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
                ( (Company)DataContext ).AddNewProduct( ProductName.Text, (ProductCategory)ProductCategoryComboBox.SelectedValue, int.Parse( ProductPrice.Text ) );
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
            return ( (Company)DataContext ).MarketPlace.Products.Any( p => p.Name == productName );
        }

        private void Button_Click_Cancel( object sender, RoutedEventArgs e )
        {
            Close();
        }
    }
}
