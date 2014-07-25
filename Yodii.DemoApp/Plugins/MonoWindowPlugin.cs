using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public abstract class MonoWindowPlugin : NotifyPropertyChangedBase, IYodiiPlugin
    {
        readonly bool _isQuickLifeTimeManagement;
        Window _window;
        readonly IYodiiEngine _engine;

        protected MonoWindowPlugin( bool isQuickLifeTimeManagement, IYodiiEngine engine )
        {
            _isQuickLifeTimeManagement = isQuickLifeTimeManagement;
            _engine = engine;
        }
        int _windowClosed;
        public bool WindowClosed()//there MUST be a better way to do this.
        {
            if( _windowClosed == 0 )
            {
                string plugin = this.GetType().ToString();
                if( _engine != null && _engine.IsRunning )
                {

                    if( _engine.LiveInfo.FindPlugin( plugin ).Capability.CanStop )
                    {
                        _engine.LiveInfo.FindPlugin( plugin ).Stop();
                        return true;
                    }
                    return false;
                }
            }
            return true;
        }

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            if( !_isQuickLifeTimeManagement )
            {
                _window = CreateWindow();
            }
            _windowClosed = 0;
            return true;
        }

        void IYodiiPlugin.Start()
        {
            if( !_isQuickLifeTimeManagement )
            {
                if( _window != null )
                    _window.Show();
            }
            else
            {
                _window = CreateWindow();
                _window.Show();
            }
            _windowClosed = 0;
        }

        void IYodiiPlugin.Stop()
        {
            if( !_isQuickLifeTimeManagement )
                HideWindow();
            else
                HideAndDestroyWindow();
            Stopping();
        }

        protected virtual void Stopping() { }

        void IYodiiPlugin.Teardown()
        {
            if( !_isQuickLifeTimeManagement )
            {
                DestroyWindow();
            }
        }

        protected bool IsQuickLifeTimeManagement { get { return _isQuickLifeTimeManagement; } }

        protected Window Window
        {
            get { return _window; }
            set
            {
                if( value == null ) throw new ArgumentNullException( "value" );
                _window = value;
            }
        }

        protected abstract Window CreateWindow();

        private void HideWindow()
        {
            if( _window != null )
                _window.Hide();
        }

        private void DestroyWindow()
        {
            if( _window != null )
            {
                _window.Close();
                _window = null;
            }
        }

        private void HideAndDestroyWindow()
        {
            _windowClosed++;
            if( _window != null )
            {
                HideWindow();
                DestroyWindow();
            }
        }
    }
}
