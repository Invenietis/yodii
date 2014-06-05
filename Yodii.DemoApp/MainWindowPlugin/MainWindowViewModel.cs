using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class MainWindowViewModel : ViewModelBase
    {
        readonly ICommand _setDiscoverer;
        readonly IYodiiEngine _engine;
        readonly DemoAppManager _manager;

        public MainWindowViewModel()
        {
            _manager = new DemoAppManager();
            _engine = _manager.Engine;
            _setDiscoverer = new RelayCommand( SetDiscoverer );
            //_manager.RunningPlugins.CollectionChanged += RunningPlugins_CollectionChanged;

        }

        private void SetDiscoverer( object param )
        {
            _manager.RetrieveGraph();
        }

        private ObservableCollection<IPluginInfo> RunningPlugins
        {
            get { return _manager.RunningPlugins; }
        }
    }
}
