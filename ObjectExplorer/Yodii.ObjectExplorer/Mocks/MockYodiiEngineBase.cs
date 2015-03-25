#if DEBUG
using System;
using PropertyChanged;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    public class MockYodiiEngine : IYodiiEngineProxy
    {
        public MockYodiiEngine()
        {
            LiveInfo = new MockLiveInfo();
        }

        public IConfigurationManager Configuration
        {
            get { throw new NotImplementedException(); }
        }

        public MockLiveInfo LiveInfo { get; private set; }

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


        #region IYodiiEngine Members

        public IYodiiEngineExternal ExternalEngine
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        #endregion

        #region IYodiiEngineProxy Members


        public event EventHandler IsRunningLockedChanged;

        public bool IsRunningLocked
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        #region IYodiiEngineBase Members


        ILiveInfo IYodiiEngineBase.LiveInfo
        {
            get { return LiveInfo; }
        }

        #endregion

        #region IYodiiEngineProxy Members


        public bool IsSelfLocked
        {
            get { return false; }
        }

        public bool SelfLock()
        {
            throw new NotImplementedException();
        }

        public void SelfUnlock()
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
#endif