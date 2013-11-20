using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using Yodii.Model;

namespace Yodii.Engine
{
    public class YodiiEngine : IYodiiEngine
    {
        IDiscoveredInfo _discoveredInfo;
        readonly ConfigurationManager _manager;

        public YodiiEngine()
        {
            _discoveredInfo = EmptyDiscoveredInfo.Empty;
            _manager = new ConfigurationManager();
        }

        public IDiscoveredInfo DiscoveredInfo
        {
            get { return _discoveredInfo; }
            set
            {
                if( value == null ) value = EmptyDiscoveredInfo.Empty;
                if ( value != _discoveredInfo )
                {
                    _discoveredInfo = value;
                    RaisePropertyChanged();
                }
            }
        }

        public ConfigurationManager ConfigurationManager
        {
            get { return _manager; }
        }

        public IYodiiEngineResult Start()
        {
        }

        public void Stop()
        {
        }


        void RaisePropertyChanged( [CallerMemberName]string propertyName = null )
        {
            throw new NotImplementedException();
        }

    }
}
