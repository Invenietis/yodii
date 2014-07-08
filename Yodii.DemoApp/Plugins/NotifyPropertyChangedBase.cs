using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Yodii.DemoApp
{
    public abstract class NotifyPropertyChangedBase : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged( [CallerMemberName] string caller = null )
        {
            var h =PropertyChanged;
            Debug.Assert( caller != null );
            if( h != null )
            {
                h( this, new PropertyChangedEventArgs( caller ) );
            }
        }
    }
}
