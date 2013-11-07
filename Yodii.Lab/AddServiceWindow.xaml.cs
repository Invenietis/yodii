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
using System.Windows.Shapes;
using CK.Core;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Window used to add and create a service.
    /// </summary>
    public partial class AddServiceWindow : Window, INotifyPropertyChanged
    {
        public event EventHandler<NewServiceEventArgs> NewServiceCreated;

        bool _hasGeneralization;
        string _serviceName;
        IServiceInfo _generalization;

        public ICKObservableReadOnlyCollection<IServiceInfo> AvailableServices
        {
            get;
            private set;
        }

        public bool HasGeneralization
        {
            get { return _hasGeneralization; }
            set
            {
                if( value != _hasGeneralization )
                {
                    _hasGeneralization = value;
                    RaisePropertyChanged( "HasGeneralization" );
                }
            }
        }

        public string NewServiceName
        {
            get { return _serviceName; }
            set
            {
                if( value != _serviceName )
                {
                    _serviceName = value;
                    RaisePropertyChanged( "NewServiceName" );
                }
            }
        }

        public IServiceInfo SelectedGeneralization
        {
            get { return _generalization; }
            set
            {
                if( value != _generalization )
                {
                    _generalization = value;
                    RaisePropertyChanged( "SelectedGeneralization" );
                }
            }
        }

        public AddServiceWindow( ICKObservableReadOnlyCollection<IServiceInfo> availableServices, IServiceInfo selectedService = null )
        {
            AvailableServices = availableServices;

            this.DataContext = this;

            if( selectedService != null )
            {
                HasGeneralization = true;
                SelectedGeneralization = selectedService;
            }

            InitializeComponent();

            this.NewServiceNameTextBox.Focus();
        }

        private bool RaiseNewService( string name, IServiceInfo generalization = null )
        {
            if( NewServiceCreated != null )
            {
                NewServiceEventArgs args = new NewServiceEventArgs()
                {
                    ServiceName = name,
                    Generalization = generalization
                };

                // Raise event
                NewServiceCreated( this, args );

                if( args.CancelReason != null )
                {
                    MessageBox.Show( args.CancelReason, "Couldn't create service", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK );
                    return false;
                }
            }

            return true;
        }

        #region INotifyPropertyChanged utilities

        /// <summary>
        /// Occurs when [property changed].
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Throw new PropertyChanged.
        /// </summary>
        /// <param name="caller">Fill with Member name, when called from a property.</param>
        protected void RaisePropertyChanged( string caller )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        #endregion INotifyPropertyChanged utilities

        private void CancelButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void CreateButton_Click( object sender, RoutedEventArgs e )
        {
            if( String.IsNullOrWhiteSpace( NewServiceName ) ) return;

            IServiceInfo generalization = SelectedGeneralization;

            if( !HasGeneralization )
                generalization = null;

            if( RaiseNewService( NewServiceName, generalization ) )
            {
                this.Close();
            }
        }

    }

    public class NewServiceEventArgs : EventArgs
    {
        public string ServiceName { get; internal set; }
        public IServiceInfo Generalization { get; internal set; }
        public string CancelReason { get; set; }
    }
}
