using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Wpf.Tests
{
    public class YodiiEngineWrapper : IYodiiEngine
    {
        readonly IYodiiEngineExternal _engine;
        readonly string _pluginFullName;

        public YodiiEngineWrapper( IYodiiEngineExternal e, string pluginFullName )
        {
            _engine = e;
            _pluginFullName = pluginFullName;
        }
        public YodiiEngineWrapper( IYodiiEngineExternal e )
            : this( e, String.Empty )
        {
        }


        #region IYodiiEngine Members

        IYodiiEngineExternal IYodiiEngine.ExternalEngine
        {
            get { return _engine; }
        }

        IConfigurationManager IYodiiEngineBase.Configuration
        {
            get { return _engine.Configuration; }
        }

        ILiveInfo IYodiiEngineBase.LiveInfo
        {
            get { return _engine.LiveInfo; }
        }

        IYodiiEngineResult IYodiiEngineBase.StartItem( ILiveYodiiItem pluginOrService, StartDependencyImpact impact, string callerKey )
        {
            return _engine.StartItem( pluginOrService, impact, callerKey ?? _pluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StopItem( ILiveYodiiItem pluginOrService, string callerKey )
        {
            return _engine.StopItem( pluginOrService, callerKey ?? _pluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StartPlugin( string pluginFullName, StartDependencyImpact impact, string callerKey )
        {
            return _engine.StartPlugin( pluginFullName, impact, callerKey ?? _pluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StopPlugin( string pluginFullName, string callerKey )
        {
            return _engine.StopPlugin( pluginFullName, callerKey ?? _pluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StartService( string serviceFullName, StartDependencyImpact impact, string callerKey )
        {
            return _engine.StartService( serviceFullName, impact, callerKey ?? _pluginFullName );
        }

        IYodiiEngineResult IYodiiEngineBase.StopService( string serviceFullName, string callerKey )
        {
            return _engine.StopService( serviceFullName, callerKey ?? _pluginFullName );
        }

        #endregion
    }
}
