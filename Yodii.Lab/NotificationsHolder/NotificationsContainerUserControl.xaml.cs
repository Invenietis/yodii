#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\NotificationsHolder\NotificationsContainerUserControl.xaml.cs) is part of CiviKey. 
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
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Yodii.Lab.NotificationsHolder;

namespace Yodii.Lab
{
    /// <summary>
    /// Interaction logic for NotificationsContainerUserControl.xaml
    /// </summary>
    internal partial class NotificationsContainerUserControl : UserControl, INotificationManager
    {
        private const byte MAX_NOTIFICATIONS = 50;
        private int count;
        private readonly Notifications buffer = new Notifications();

        /// <summary>
        /// Notification list.
        /// </summary>
        public Notifications Notifications = new Notifications();

        /// <summary>
        /// Creates a new NotificationsContainerUserControl.
        /// </summary>
        public NotificationsContainerUserControl()
        {
            InitializeComponent();
            NotificationsControl.DataContext = Notifications;
        }

        /// <summary>
        /// Adds a notification to the queue, and display it if applicable.
        /// </summary>
        /// <param name="notification">Notification to add</param>
        public void AddNotification( Notification notification )
        {
            notification.Id = count++;
            if( Notifications.Count + 1 > MAX_NOTIFICATIONS )
                buffer.Add( notification );
            else
                Notifications.Add( notification );
            //Show window if there're notifications
            if( Notifications.Count > 0 )
                this.Visibility = System.Windows.Visibility.Visible;
        }

        /// <summary>
        /// Removes a notification from the queue, clearing it.
        /// </summary>
        /// <param name="notification">Notification to remove.</param>
        public void RemoveNotification( Notification notification )
        {
            if( Notifications.Contains( notification ) )
                Notifications.Remove( notification );
            if( buffer.Count > 0 )
            {
                Notifications.Add( buffer[0] );
                buffer.RemoveAt( 0 );
            }
            //Close window if there's nothing to show
            if( Notifications.Count < 1 )
                this.Visibility = System.Windows.Visibility.Collapsed;
        }

        private void NotificationWindowSizeChanged( object sender, SizeChangedEventArgs e )
        {
            if( e.NewSize.Height != 0.0 )
                return;
            var element = sender as Grid;
            RemoveNotification( Notifications.First(
              n => n.Id == Int32.Parse( element.Tag.ToString() ) ) );
        }

    }
}
