using System;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public abstract class MonoWindowPlugin : NotifyPropertyChangedBase, IYodiiPlugin
    {
        readonly bool _isQuickLifeTimeManagement;
        Window _window;

        protected MonoWindowPlugin( bool isQuickLifeTimeManagement )
        {
            _isQuickLifeTimeManagement = isQuickLifeTimeManagement;
        }

        bool IYodiiPlugin.Setup( PluginSetupInfo info )
        {
            if( !_isQuickLifeTimeManagement )
            {
                _window = CreateWindow();
            }
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
        }

        void IYodiiPlugin.Stop()
        {
            if( !_isQuickLifeTimeManagement )
                HideWindow();
            else
                HideAndDestroyWindow();
            Stopping();
        }

        protected virtual void Stopping(){}

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
                if( value == null ) throw new ArgumentNullException("value");
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
            if( _window != null )
            {
                HideWindow();
                DestroyWindow();
            }
        }
    }
}
