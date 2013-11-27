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
    public partial class NotificationsContainerUserControl : UserControl, INotificationManager
    {
        private const byte MAX_NOTIFICATIONS = 50;
        private int count;
        public Notifications Notifications = new Notifications();
        private readonly Notifications buffer = new Notifications();

        public NotificationsContainerUserControl()
        {
            InitializeComponent();
            NotificationsControl.DataContext = Notifications;
        }

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
