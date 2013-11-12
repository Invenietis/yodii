using System;
using System.Collections.Generic;
using System.ComponentModel;
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
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for PluginPropertyPanel.xaml
    /// </summary>
    public partial class PluginPropertyPanel : UserControl
    {
        public static readonly DependencyProperty LivePluginInfoProperty = 
            DependencyProperty.Register( "LivePluginInfo", typeof( LivePluginInfo ),
            typeof( PluginPropertyPanel ), new FrameworkPropertyMetadata( LivePluginInfoChanged )
            );

        private static void LivePluginInfoChanged( DependencyObject d, DependencyPropertyChangedEventArgs e )
        {
            PluginPropertyPanel p = d as PluginPropertyPanel;
            if( e.NewValue == null )
            {
                // Explicit binding removal is necessary here, or WPF will null the ComboBox binding, effectively removing PluginInfo.Service.
                p.PluginServiceComboBox.ClearValue( ComboBox.SelectedValueProperty );
            }
        }

        public static readonly DependencyProperty ServiceInfosProperty = 
            DependencyProperty.Register( "ServiceInfos", typeof( ICKObservableReadOnlyCollection<IServiceInfo> ),
            typeof( PluginPropertyPanel )
            );

        public ICKObservableReadOnlyCollection<IServiceInfo> ServiceInfos
        {
            get { return (ICKObservableReadOnlyCollection<IServiceInfo>)GetValue( ServiceInfosProperty ); }
            set { SetValue( ServiceInfosProperty, value ); }
        }

        public LivePluginInfo LivePluginInfo
        {
            get { return (LivePluginInfo)GetValue( LivePluginInfoProperty ); }
            set { SetValue( LivePluginInfoProperty, value ); }
        }

        public PluginPropertyPanel()
        {
            InitializeComponent();
        }


        private void DeleteReferenceButton_Click( object sender, RoutedEventArgs e )
        {
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement;
            MockServiceReferenceInfo serviceRef = parentElement.DataContext as MockServiceReferenceInfo;

            serviceRef.Owner.InternalServiceReferences.Remove( serviceRef );
        }

        private bool isResettingSelection; // Kept if we ever want to cancel selection, but not used yet
        private void ReferenceRequirementComboBox_SelectionChanged( object sender, System.Windows.Controls.SelectionChangedEventArgs e )
        {
            if( !isResettingSelection && e.RemovedItems.Count == 1 && e.AddedItems.Count == 1 )
            {
                ComboBox box = sender as ComboBox;
                FrameworkElement parentElement = box.Parent as FrameworkElement;
                MockServiceReferenceInfo serviceRef = parentElement.DataContext as MockServiceReferenceInfo;

                RunningRequirement oldReq = serviceRef.Requirement;
                RunningRequirement newReq = (RunningRequirement)e.AddedItems[0];

                serviceRef.Requirement = newReq;
            }
            isResettingSelection = false;
        }

        private void CreateReferenceButton_Click( object sender, RoutedEventArgs e )
        {
            if( LivePluginInfo == null ) return;
            Button button = sender as Button;
            FrameworkElement parentElement = button.Parent as FrameworkElement;

            ComboBox requirementComboBox = parentElement.FindName( "NewReferenceRequirementComboBox" ) as ComboBox;
            ComboBox serviceComboBox = parentElement.FindName( "NewReferenceServiceComboBox" ) as ComboBox;

            ServiceInfo service = serviceComboBox.SelectedItem as ServiceInfo;
            RunningRequirement req = (RunningRequirement)requirementComboBox.SelectedItem;

            LivePluginInfo.PluginInfo.InternalServiceReferences.Add( new MockServiceReferenceInfo( LivePluginInfo.PluginInfo, service, req ) );
        }

    }
}
