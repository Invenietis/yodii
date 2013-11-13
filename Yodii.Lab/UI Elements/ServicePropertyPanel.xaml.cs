using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Lab.Utils;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for ServicePropertyPanel.xaml
    /// </summary>
    public partial class ServicePropertyPanel : UserControl
    {
        public static readonly DependencyProperty LiveServiceInfoProperty = 
            DependencyProperty.Register( "LiveServiceInfo", typeof( LiveServiceInfo ),
            typeof( ServicePropertyPanel ), new PropertyMetadata( LiveServiceInfoChanged )
            );

        private static void LiveServiceInfoChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            ServicePropertyPanel p = d as ServicePropertyPanel;
            if( e.NewValue == null )
            {
                // Explicit binding removal is necessary here, or WPF will null the ComboBox binding, effectively removing ServiceInfo.Generalization.
                p.GeneralizationComboBox.ClearValue( ComboBox.SelectedValueProperty );
            }
        }

        public static readonly DependencyProperty ServiceInfoManagerProperty = 
            DependencyProperty.Register( "ServiceInfoManager", typeof( ServiceInfoManager ),
            typeof( ServicePropertyPanel )
            );

        internal ServiceInfoManager ServiceInfoManager
        {
            get { return (ServiceInfoManager)GetValue( ServiceInfoManagerProperty ); }
            set { SetValue( ServiceInfoManagerProperty, value ); }
        }

        public LiveServiceInfo LiveServiceInfo
        {
            get { return (LiveServiceInfo)GetValue( LiveServiceInfoProperty ); }
            set { SetValue( LiveServiceInfoProperty, value ); }
        }

        public ServicePropertyPanel()
        {
            InitializeComponent();
        }

        private void ServiceNamePropertyTextBox_LostFocus( object sender, RoutedEventArgs e )
        {
            UpdateServicePropertyNameWithTextbox( sender as System.Windows.Controls.TextBox );
        }

        private void ServiceNamePropertyTextBox_KeyDown( object sender, System.Windows.Input.KeyEventArgs e )
        {
            if( e.Key == System.Windows.Input.Key.Enter )
            {
                UpdateServicePropertyNameWithTextbox( sender as System.Windows.Controls.TextBox );
            }
        }

        private void UpdateServicePropertyNameWithTextbox( System.Windows.Controls.TextBox textBox )
        {
            LiveServiceInfo liveService = textBox.DataContext as LiveServiceInfo;

            if( liveService.ServiceInfo.ServiceFullName == textBox.Text ) return;
            if( String.IsNullOrWhiteSpace( textBox.Text ) ) return;

            DetailedOperationResult r = ServiceInfoManager.RenameService( liveService.ServiceInfo, textBox.Text );

            if( !r )
            {
                textBox.Text = liveService.ServiceInfo.ServiceFullName;
                MessageBox.Show( String.Format( "Couldn't change service name.\n{0}", r.Reason ), "Couldn't change service name", MessageBoxButton.OK, MessageBoxImage.Exclamation, MessageBoxResult.OK );
            }
        }

        private void HasGeneralizationCheckbox_Unchecked( object sender, RoutedEventArgs e )
        {
            if( LiveServiceInfo == null ) return;
            LiveServiceInfo.ServiceInfo.Generalization = null;
        }
    }
}
