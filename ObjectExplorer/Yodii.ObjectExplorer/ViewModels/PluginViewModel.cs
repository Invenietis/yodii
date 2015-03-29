using System;
using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Reflection;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class PluginViewModel : YodiiItemViewModelBase
    {
        [AllowNull]
        public ILivePluginInfo Plugin { get { return (ILivePluginInfo)LiveItem; } }

        public PluginViewModel()
        {

        }

        [AllowNull]
        public ServiceViewModel Service
        {
            get;
            internal set;
        }

        public override YodiiItemViewModelBase Parent
        {
            get
            {
                return Service;
            }
        }

        ObservableCollection<YodiiItemViewModelBase> _children = new ObservableCollection<YodiiItemViewModelBase>();
        public override ObservableCollection<YodiiItemViewModelBase> Children
        {
            get
            {
                return _children;
            }
        }

        public override IAssemblyInfo AssemblyInfo
        {
            get
            {
                return Plugin.PluginInfo.AssemblyInfo;
            }
        }

        public override string ToString()
        {
            string name = Plugin != null ? DisplayName : "(null)";

            return String.Format( "[Plugin: {0}]", name );
        }
    }
}