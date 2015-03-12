using System;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ServiceViewModel : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public ILiveServiceInfo Service { get; private set; }

        public ServiceViewModel()
        {

        }

        public void LoadLiveService( ILiveServiceInfo service )
        {
            if( Service != null ) { throw new InvalidOperationException( "Cannot load a Service twice." ); }
            Service = service;
        }
    }
}