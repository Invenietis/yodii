using System;
using System.Windows;
using Yodii.DemoApp;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public class Plugin1 : IYodiiPlugin
    {
        Plugin1Window _window;

        public Plugin1()
        {

        }

        public bool Setup( PluginSetupInfo info )
        {
            throw new NotImplementedException();
        }

        public void Start()
        {
            Plugin1Window _window = new Plugin1Window();
            _window.Activate();
        }

        public void Teardown()
        {
            throw new NotImplementedException();
        }

        public void Stop()
        {
            if( _window != null )
            {
            }
        }
    }
}
