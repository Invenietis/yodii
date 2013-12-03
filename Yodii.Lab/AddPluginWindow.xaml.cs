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
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for AddPluginWindow.xaml
    /// </summary>
    public partial class AddPluginWindow : Window, INotifyPropertyChanged
    {
        public event EventHandler<NewPluginEventArgs> NewPluginCreated;
        readonly Dictionary<IServiceInfo,DependencyRequirement> _serviceReferences; // TODO

        string _pluginName;
        string _pluginGuidText;
        IServiceInfo _service;

        public AddPluginWindow( ICKObservableReadOnlyCollection<IServiceInfo> availableServices, IServiceInfo selectedService = null )
        {
            _serviceReferences = new Dictionary<IServiceInfo, DependencyRequirement>();
            AvailableServices = availableServices;

            NewPluginGuidText = Guid.NewGuid().ToString();

            if( selectedService != null )
            {
                SelectedService = selectedService;
            }

            this.DataContext = this;

            InitializeComponent();

            this.NewPluginNameTextBox.Focus();
        }

        #region Properties

        public ICKObservableReadOnlyCollection<IServiceInfo> AvailableServices
        {
            get;
            private set;
        }

        public bool HasService
        {
            get { return SelectedService != null; }
        }

        public string NewPluginName
        {
            get { return _pluginName; }
            set
            {
                if( value != _pluginName )
                {
                    _pluginName = value;
                    RaisePropertyChanged( "NewPluginName" );
                }
            }
        }

        public string NewPluginGuidText
        {
            get { return _pluginGuidText; }
            set
            {
                if( value != _pluginGuidText )
                {
                    _pluginGuidText = value;
                    RaisePropertyChanged( "NewPluginGuidText" );
                }
            }
        }

        public IServiceInfo SelectedService
        {
            get { return _service; }
            set
            {
                if( value != _service )
                {
                    _service = value;
                    RaisePropertyChanged( "SelectedService" );
                    RaisePropertyChanged( "HasService" );
                }
            }
        }


        #endregion Properties

        private void CreateButton_Click( object sender, RoutedEventArgs e )
        {
            Guid newGuid;

            if( !Guid.TryParse( NewPluginGuidText, out newGuid ) )
            {
                var mbResult = MessageBox.Show( "Invalid GUID.\nPress OK to generate a random one.", "Invalid GUID", MessageBoxButton.OKCancel, MessageBoxImage.Stop );
                if( mbResult == MessageBoxResult.OK )
                {
                    NewPluginGuidText = Guid.NewGuid().ToString();
                }
                return; // StopByCommand here on wrong GUID.
            }

            IServiceInfo service = SelectedService;
            if( !HasService ) service = null;

            string newPluginName = NewPluginName;
            if( newPluginName == null ) newPluginName = String.Empty;

            if( RaiseNewPlugin( newGuid, newPluginName, service, _serviceReferences ) )
            {
                this.Close();
            }
        }

        private bool RaiseNewPlugin( Guid newGuid, string newPluginName, IServiceInfo service, Dictionary<IServiceInfo, DependencyRequirement> serviceReferences )
        {
            if( NewPluginCreated != null )
            {
                NewPluginEventArgs args = new NewPluginEventArgs()
                {
                    PluginId = newGuid,
                    PluginName = newPluginName,
                    Service = service,
                    ServiceReferences = serviceReferences
                };

                // Raise event
                NewPluginCreated( this, args );

                if( args.CancelReason != null )
                {
                    MessageBox.Show( args.CancelReason, "Couldn't create plugin", MessageBoxButton.OK, MessageBoxImage.Stop, MessageBoxResult.OK );
                    return false;
                }
            }

            return true;
        }

        private void CancelButton_Click( object sender, RoutedEventArgs e )
        {
            this.Close();
        }

        private void ClearServiceButton_Click( object sender, RoutedEventArgs e )
        {
            SelectedService = null;
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

    }

    public class NewPluginEventArgs : EventArgs
    {
        public Guid PluginId { get; internal set; }
        public string PluginName { get; internal set; }
        public IServiceInfo Service { get; internal set; }

        public Dictionary<IServiceInfo, DependencyRequirement> ServiceReferences { get; internal set; }

        public string CancelReason { get; set; }
    }
}
