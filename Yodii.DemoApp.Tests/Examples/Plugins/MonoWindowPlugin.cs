using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public abstract class MonoWindowPlugin : IYodiiPlugin
    {
        readonly bool _runningLifetimeWindow;
        Window _window;
 
        protected MonoWindowPlugin( bool runningLifetimeWindow, Window window )
        {
            _runningLifetimeWindow = runningLifetimeWindow;
            _window = window;
        }

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            if( !_runningLifetimeWindow )
            {
                _window = CreateAndShowWindow();
            }

            return true;
        }

        void IYodiiPlugin.Start()
        {
            if( _runningLifetimeWindow )
            {
                _window = CreateAndShowWindow();
            }
        }


        void IYodiiPlugin.Stop()
        {
            if( _runningLifetimeWindow )
            {
                DestroyWindow();
            }
        }
        
        void IYodiiPlugin.Teardown()
        {
            if( !_runningLifetimeWindow )
            {
                DestroyWindow();
                _window = null;
            }
        }

        protected bool RunningLifetimeWindow { get { return _runningLifetimeWindow; } }

        protected Window Window { get { return _window; } }

        protected abstract Window CreateAndShowWindow();

        protected abstract void DestroyWindow();
    }
}
