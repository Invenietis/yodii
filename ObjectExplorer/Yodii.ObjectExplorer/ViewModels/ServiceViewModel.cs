using System;
using System.Diagnostics;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ServiceViewModel : YodiiItemViewModelBase<ILiveServiceInfo>
    {
        [AllowNull]
        public ILiveServiceInfo Service { get { return YodiiItem; } }

        public ServiceViewModel()
        {

        }

        protected override Assembly GetItemAssembly()
        {
            return Assembly.Load( Service.ServiceInfo.AssemblyInfo.AssemblyName );
        }
    }
}