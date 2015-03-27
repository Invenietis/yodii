using System;
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

        public override IAssemblyInfo AssemblyInfo
        {
            get
            {
                return Plugin.PluginInfo.AssemblyInfo;
            }
        }
    }
}