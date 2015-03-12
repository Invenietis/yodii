using System;
using NullGuard;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.ViewModels
{
    [ImplementPropertyChanged]
    public class PluginViewModel : EmptyPropertyChangedHandler
    {
        [AllowNull]
        public ILivePluginInfo Plugin { get; private set; }

        public PluginViewModel()
        {

        }

        public void LoadLivePlugin( ILivePluginInfo plugin )
        {
            Plugin = plugin;
        }
    }
}