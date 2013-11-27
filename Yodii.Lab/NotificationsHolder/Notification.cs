using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows;

namespace Yodii.Lab
{
    public class Notification : INotifyPropertyChanged
    {
        private string message;
        public string Message
        {
            get { return message; }

            set
            {
                if( message == value ) return;
                message = value;
                OnPropertyChanged( "Message" );
                OnPropertyChanged( "MessageVisibility" );
            }
        }

        public Visibility MessageVisibility
        {
            get
            {
                if( String.IsNullOrWhiteSpace( Message ) ) return Visibility.Collapsed;
                else return Visibility.Visible;
            }
        }

        private int id;
        public int Id
        {
            get { return id; }

            set
            {
                if( id == value ) return;
                id = value;
                OnPropertyChanged( "Id" );
            }
        }

        private string imageUrl;
        public string ImageUrl
        {
            get { return imageUrl; }

            set
            {
                if( imageUrl == value ) return;
                imageUrl = value;
                OnPropertyChanged( "ImageUrl" );
            }
        }

        private string title;
        public string Title
        {
            get { return title; }

            set
            {
                if( title == value ) return;
                title = value;
                OnPropertyChanged( "Title" );
            }
        }

        protected virtual void OnPropertyChanged( string propertyName )
        {
            var handler = PropertyChanged;
            if( handler != null ) handler( this, new PropertyChangedEventArgs( propertyName ) );
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    public class Notifications : ObservableCollection<Notification> { }
}
