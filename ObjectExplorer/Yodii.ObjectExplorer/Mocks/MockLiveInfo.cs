#if DEBUG
using System;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    public class MockLiveInfo : ILiveInfo
    {
        public MockLiveInfo()
        {
            Plugins = new CK.Core.CKObservableSortedArrayKeyList<ILivePluginInfo, string>( x => x.FullName, false );
            Services = new CK.Core.CKObservableSortedArrayKeyList<ILiveServiceInfo, string>( x => x.FullName, false );
        }

        public CK.Core.CKObservableSortedArrayKeyList<ILivePluginInfo, string> Plugins { get; private set; }
        CK.Core.IObservableReadOnlyList<ILivePluginInfo> ILiveInfo.Plugins
        {
            get { return Plugins; }
        }

        public CK.Core.CKObservableSortedArrayKeyList<ILiveServiceInfo, string> Services { get; private set; }
        CK.Core.IObservableReadOnlyList<ILiveServiceInfo> ILiveInfo.Services
        {
            get { return Services; }
        }

        public CK.Core.IObservableReadOnlyList<YodiiCommand> YodiiCommands
        {
            get { throw new NotImplementedException(); }
        }

        public ILiveServiceInfo FindService( string serviceFullName )
        {
            throw new NotImplementedException();
        }

        public ILivePluginInfo FindPlugin( string pluginFullName )
        {
            throw new NotImplementedException();
        }

        public ILiveYodiiItem FindYodiiItem( string pluginOrserviceFullName )
        {
            throw new NotImplementedException();
        }

        public IYodiiEngineResult RevokeCaller( string callerKey = null )
        {
            throw new NotImplementedException();
        }
    }
}
#endif