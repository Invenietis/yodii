using System;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class ServiceViewModel : YodiiItemViewModelBase
    {
        [AllowNull]
        public ILiveServiceInfo Service { get { return (ILiveServiceInfo)LiveItem; } }

        public ServiceViewModel()
        {

        }
        public override IAssemblyInfo AssemblyInfo
        {
            get
            {
                return Service.ServiceInfo.AssemblyInfo;
            }
        }

        [AllowNull]
        public ServiceViewModel Generalization
        {
            get;
            internal set;
        }

        [AllowNull]
        public override YodiiItemViewModelBase Parent
        {
            get
            {
                return Generalization;
            }
        }

        ObservableCollection<YodiiItemViewModelBase> _children = new ObservableCollection<YodiiItemViewModelBase>();
        public override ObservableCollection<YodiiItemViewModelBase> Children
        {
            get
            {
                // Collection is internally handled by the EngineViewModel
                return _children;
            }
        }

        public override string ToString()
        {
            string name = Service != null ? DisplayName : "(null)";

            return String.Format( "[Service: {0}]", name );
        }
    }
}