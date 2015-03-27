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
    public class PluginViewModel : YodiiItemViewModelBase<ILivePluginInfo>
    {
        [AllowNull]
        public ILivePluginInfo Plugin { get { return YodiiItem; } }

        public PluginViewModel()
        {

        }

        protected override Assembly GetItemAssembly()
        {
            return Assembly.Load( Plugin.PluginInfo.AssemblyInfo.AssemblyName );
        }
    }
}