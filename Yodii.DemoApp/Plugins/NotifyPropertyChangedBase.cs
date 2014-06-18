using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace Yodii.DemoApp
{
    public abstract class NotifyPropertyChangedBase
    {
        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged( [CallerMemberName] string caller = null )
        {
            Debug.Assert( caller != null );
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( caller ) );
            }
        }
    }
}
