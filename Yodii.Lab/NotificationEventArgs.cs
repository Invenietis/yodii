using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
