using System;

namespace Yodii.Lab
{
    public class NotificationEventArgs : EventArgs
    {
        public readonly Notification Notification;

        public NotificationEventArgs( Notification n )
        {
            Notification = n;
        }
    }
}
