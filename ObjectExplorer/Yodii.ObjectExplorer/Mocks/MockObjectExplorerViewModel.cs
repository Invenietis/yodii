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

        public MockObjectExplorerViewModel()
            : base()
        {
            MockEngineHelper.StartDesignTimeYodiiEngine();

            this.LoadEngine( new MockYodiiEngineProxy( MockEngineHelper.Engine ) );
        }
#endif
    }

}
