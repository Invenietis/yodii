using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using Yodii.Model;

namespace Yodii.DemoApp
{
    public abstract class MonoWindowPlugin : YodiiPluginBase, INotifyPropertyChanged
    {
        readonly bool _isQuickLifeTimeManagement;
        Window _window;

        protected MonoWindowPlugin( bool isQuickLifeTimeManagement )
        {
            _isQuickLifeTimeManagement = isQuickLifeTimeManagement;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void RaisePropertyChanged( [CallerMemberName] string caller = null )
        {
            var h =PropertyChanged;
            Debug.Assert( caller != null );
            if( h != null )
            {
                h( this, new PropertyChangedEventArgs( caller ) );
            }
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            if( !_isQuickLifeTimeManagement )
            {
                _window = CreateWindow();
            }
            base.PluginPreStart( c );
        }
        protected override void PluginStart( IStartContext c )
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
            base.PluginStart( c );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            if( !_isQuickLifeTimeManagement )
                HideWindow();
            else
                HideAndDestroyWindow();
        }

        protected override void PluginStop( IStopContext c )
        {
            if( !_isQuickLifeTimeManagement )
            {
                DestroyWindow();
            }
            base.PluginStop( c );
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
