#if DEBUG
using System;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    public class MockYodiiEngineBase : IYodiiEngineBase
    {
        public MockYodiiEngineBase()
        {
            LiveInfo = new MockLiveInfo();
        }

        public IConfigurationManager Configuration
        {
            get { throw new NotImplementedException(); }
        }

        public MockLiveInfo LiveInfo { get; private set; }

        ILiveInfo IYodiiEngineBase.LiveInfo
        {
            get{ return LiveInfo; }
        }

        public IYodiiEngineResult StartItem( ILiveYodiiItem pluginOrService, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            throw new NotImplementedException();
        }

        public IYodiiEngineResult StopItem( ILiveYodiiItem pluginOrService, string callerKey = null )
        {
            throw new NotImplementedException();
        }

        public IYodiiEngineResult StartPlugin( string pluginFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            throw new NotImplementedException();
        }

        public IYodiiEngineResult StopPlugin( string pluginFullName, string callerKey = null )
        {
            throw new NotImplementedException();
        }

        public IYodiiEngineResult StartService( string serviceFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            throw new NotImplementedException();
        }

        public IYodiiEngineResult StopService( string serviceFullName, string callerKey = null )
        {
            throw new NotImplementedException();
        }

    }
}
#endif