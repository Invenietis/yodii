using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Discoverer
{
    [Plugin( "dddddddddddddddddddddddddddddddd", PublicName = "Plugin1" )]
    public class Plugin1 : IYodiiPlugin, Service2, Service1
    {
        readonly string _pluginFullName;

        public Plugin1( string pluginFullName )
        {
            _pluginFullName = pluginFullName;
        }

        public Plugin1()
        {
        }

        #region InterfaceMethods
        public bool Setup( PluginSetupInfo info )
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            throw new NotImplementedException();
        }

        public void Teardown()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            throw new NotImplementedException();
        }
        #endregion
    }
}
