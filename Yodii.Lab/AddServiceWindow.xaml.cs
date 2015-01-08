#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\AddServiceWindow.xaml.cs) is part of CiviKey. 
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
using System.ComponentModel;
using System.Diagnostics;
using System.Windows;
using CK.Core;
using Yodii.Model;

namespace Yodii.Lab
{
    /// <summary>
    /// Window used to add and create a service.
    /// </summary>
    internal partial class AddServiceWindow : Window, INotifyPropertyChanged
    {
        public event EventHandler<NewServiceEventArgs> NewServiceCreated;

        string _serviceName;
        IServiceInfo _generalization;

        public ICKObservableReadOnlyCollection<IServiceInfo> AvailableServices
        {
            get;
            private set;
        }

        public bool HasGeneralization
        {
            get { return SelectedGeneralization != null; }
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
                    RaisePropertyChanged( "HasGeneralization" );
                }
            }
        }

        public AddServiceWindow( ICKObservableReadOnlyCollection<IServiceInfo> availableServices, IServiceInfo selectedService = null )
        {
            AvailableServices = availableServices;

            this.DataContext = this;

            if( selectedService != null )
            {
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

        private void ClearGeneralizationButton_Click( object sender, RoutedEventArgs e )
        {
            SelectedGeneralization = null;
        }

    }

    internal class NewServiceEventArgs : EventArgs
    {
        public string ServiceName { get; internal set; }
        public IServiceInfo Generalization { get; internal set; }
        public string CancelReason { get; set; }
    }
}
