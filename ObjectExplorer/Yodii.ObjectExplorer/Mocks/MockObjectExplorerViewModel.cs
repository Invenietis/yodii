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
            MockYodiiEngine engineMock = new MockYodiiEngine();
            engineMock.LiveInfo.Plugins.Add( new MockLivePluginInfo() { FullName = "Yodii.ObjectExplorer.Mocks.Plugin1" } );
            engineMock.LiveInfo.Plugins.Add( new MockLivePluginInfo() { FullName = "Yodii.ObjectExplorer.Mocks.Plugin2" } );

            this.LoadEngine( engineMock );
        }
#endif
    }

}
