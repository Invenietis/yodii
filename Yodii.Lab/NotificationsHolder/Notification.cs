using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Yodii.Lab
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
