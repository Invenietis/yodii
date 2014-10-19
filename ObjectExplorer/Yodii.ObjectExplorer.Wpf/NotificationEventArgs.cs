using System;

namespace Yodii.ObjectExplorer.Wpf
{
    /// <summary>
    /// Event args for a new Notification.
    /// </summary>
    internal class NotificationEventArgs : EventArgs
    {
        /// <summary>
        /// Notification to display.
        /// </summary>
        public readonly Notification Notification;

        /// <summary>
        /// Creates a new instance of NotificationEventArgs.
        /// </summary>
        /// <param name="n">Notification to display.</param>
        public NotificationEventArgs( Notification n )
        {
            Notification = n;
        }
    }
}
