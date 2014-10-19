using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.ObjectExplorer.Wpf.NotificationsHolder
{
    interface INotificationManager
    {
        void AddNotification( Notification notification );
        void RemoveNotification( Notification notification );
    }
}
