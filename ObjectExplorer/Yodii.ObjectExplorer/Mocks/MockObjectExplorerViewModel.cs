using PropertyChanged;
using Yodii.Model;
using Yodii.ObjectExplorer.ViewModels;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    public class MockObjectExplorerViewModel : ObjectExplorerViewModel
    {
#if DEBUG
        public IYodiiEngineHost Host { get; private set; }

        public MockObjectExplorerViewModel() : base()
        {
            MockYodiiEngineBase engineBase = new MockYodiiEngineBase();
            engineBase.LiveInfo.Plugins.Add( new MockLivePluginInfo() { FullName = "Yodii.ObjectExplorer.Mocks.Plugin1" } );
            engineBase.LiveInfo.Plugins.Add( new MockLivePluginInfo() { FullName = "Yodii.ObjectExplorer.Mocks.Plugin2" } );

            this.LoadEngine( engineBase );
        }
#endif
    }

}
