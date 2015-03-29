#if DEBUG
using System;
using System.Diagnostics.CodeAnalysis;
using PropertyChanged;
using Yodii.Engine;
using Yodii.Model;

namespace Yodii.ObjectExplorer.Mocks
{
    [ImplementPropertyChanged]
    [ExcludeFromCodeCoverage]
    public class MockYodiiEngineProxy : IYodiiEngineProxy
    {
        private YodiiEngine Engine { get; set; }

        public MockYodiiEngineProxy( YodiiEngine engine )
        {
            Engine = engine;
        }


        #region IYodiiEngineProxy Members

        public IYodiiEngineExternal ExternalEngine
        {
            get { return Engine; }
        }

        public event EventHandler IsRunningLockedChanged;

        public bool IsRunningLocked
        {
            get { return true; }
        }

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

        #region IYodiiEngineBase Members

        public IConfigurationManager Configuration
        {
            get { return Engine.Configuration; }
        }

        public ILiveInfo LiveInfo
        {
            get { return Engine.LiveInfo; }
        }

        public IYodiiEngineResult StartItem( ILiveYodiiItem pluginOrService, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            return Engine.StartItem( pluginOrService, impact, callerKey );
        }

        public IYodiiEngineResult StopItem( ILiveYodiiItem pluginOrService, string callerKey = null )
        {
            return Engine.StopItem( pluginOrService, callerKey );
        }

        public IYodiiEngineResult StartPlugin( string pluginFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            return Engine.StartPlugin( pluginFullName, impact, callerKey );
        }

        public IYodiiEngineResult StopPlugin( string pluginFullName, string callerKey = null )
        {
            return Engine.StopPlugin( pluginFullName, callerKey );
        }

        public IYodiiEngineResult StartService( string serviceFullName, StartDependencyImpact impact = StartDependencyImpact.Unknown, string callerKey = null )
        {
            return Engine.StartService( serviceFullName, impact, callerKey );
        }

        public IYodiiEngineResult StopService( string serviceFullName, string callerKey = null )
        {
            return Engine.StopService( serviceFullName, callerKey );
        }

        #endregion
    }
}
#endif