#region LGPL License
/*----------------------------------------------------------------------------
* This file (ObjectExplorer\Yodii.ObjectExplorer.Wpf\NotificationsHolder\Notification.cs) is part of CiviKey. 
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
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Notification to display to the user.
    /// </summary>
    internal class Notification : ViewModelBase
    {
        private string message;

        /// <summary>
        /// Notification message.
        /// </summary>
        public string Message
        {
            get { return message; }

            set
            {
                if( message == value ) return;
                message = value;
                RaisePropertyChanged();
                RaisePropertyChanged( "MessageVisibility" );
            }
        }

        /// <summary>
        /// Whether the message should be displayed, true if message is not empty.
        /// </summary>
        public Visibility MessageVisibility
        {
            get
            {
                if( String.IsNullOrWhiteSpace( Message ) ) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private int id;
        /// <summary>
        /// Notification ID.
        /// </summary>
        public int Id
        {
            get { return id; }

            set
            {
                if( id == value ) return;
                id = value;
                RaisePropertyChanged();
            }
        }

        private string imageUrl;
        /// <summary>
        /// Notification image URI.
        /// </summary>
        public string ImageUrl
        {
            get { return imageUrl; }

            set
            {
                if( imageUrl == value ) return;
                imageUrl = value;
                RaisePropertyChanged();
            }
        }

        private string title;
        /// <summary>
        /// Norification title.
        /// </summary>
        public string Title
        {
            get { return title; }

            set
            {
                if( title == value ) return;
                title = value;
                RaisePropertyChanged();
            }
        }
    }

    internal class Notifications : ObservableCollection<Notification> { }
}
