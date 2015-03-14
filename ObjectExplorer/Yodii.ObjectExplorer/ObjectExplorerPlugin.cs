using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yodii.Model;
using Yodii.Wpf;

namespace Yodii.ObjectExplorer
{
    public class ObjectExplorerPlugin : WindowPluginBase, IObjectExplorerService
    {
        readonly IYodiiEngineProxy _engine;

        ViewModels.ObjectExplorerViewModel _viewModel;
        Windows.ObjectExplorerWindow _window;

        public ObjectExplorerPlugin( IYodiiEngineProxy e )
            : base( e )
        {
            if( e == null ) { throw new ArgumentNullException( "e" ); }

            this.AutomaticallyDisableCloseButton = false; // Handle ourselves (we use MahApps.Metro's chrome instead of Win32's one)
            this.ShowClosingFailedMessageBox = true;
            this.StopPluginWhenWindowCloses = true;

            _engine = e;
        }

        protected override Window CreateWindow()
        {
            _window = new Windows.ObjectExplorerWindow();
            _viewModel = new ViewModels.ObjectExplorerViewModel();

            _engine.IsRunningLockedChanged += _engine_IsRunningLockedChanged;
            UpdateWindowCloseButton();

            _viewModel.LoadEngine( _engine );
            _window.DataContext = _viewModel;

            return _window;
        }

        void _engine_IsRunningLockedChanged( object sender, EventArgs e )
        {
            _window.Dispatcher.Invoke( UpdateWindowCloseButton );
        }

        void UpdateWindowCloseButton()
        {
            _window.IsCloseButtonEnabled = !_engine.IsRunningLocked;
        }
    }
}
