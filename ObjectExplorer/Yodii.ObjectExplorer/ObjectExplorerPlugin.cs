using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Yodii.ConfigurationManager;
using Yodii.Model;
using Yodii.Wpf;

namespace Yodii.ObjectExplorer
{
    [YodiiPlugin( DisplayName = "Object explorer", Description = "Yodii object explorer." )]
    public class ObjectExplorerPlugin : WindowPluginBase, IObjectExplorerService
    {
        readonly IYodiiEngineProxy _engine;

        ViewModels.ObjectExplorerViewModel _viewModel;
        Windows.ObjectExplorerWindow _window;

        readonly IService<IConfigurationManagerService> _configurationManagerService;

        public ObjectExplorerPlugin( IYodiiEngineProxy e, IOptionalService<IConfigurationManagerService> configurationManagerService )
            : base( e )
        {
            if( e == null ) { throw new ArgumentNullException( "e" ); }
            if( configurationManagerService == null ) { throw new ArgumentNullException( "configurationManagerService" ); }

            this.AutomaticallyDisableCloseButton = false; // Handle ourselves (we use MahApps.Metro's chrome instead of Win32's one)
            this.ShowClosingFailedMessageBox = true;
            this.StopPluginWhenWindowCloses = true;

            _engine = e;
            _configurationManagerService = configurationManagerService;
        }

        protected override Window CreateWindow()
        {
            IConfigurationManagerWrapper w = new ConfigurationManagerWrapper( _configurationManagerService, _engine );

            _window = new Windows.ObjectExplorerWindow(w);
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

    public interface IConfigurationManagerWrapper
    {
        void ShowOrActivate();
    }

    class ConfigurationManagerWrapper : IConfigurationManagerWrapper
    {
        readonly IService<IConfigurationManagerService> _configManager;
        readonly IYodiiEngineProxy _engine;

        public ConfigurationManagerWrapper( IService<IConfigurationManagerService> configManager, IYodiiEngineProxy engine )
        {
            _configManager = configManager;
            _engine = engine;
        }

        public void ShowOrActivate()
        {
            if( _configManager.IsStartingOrStarted() )
            {
                _configManager.Service.ActivateWindow();
            }
            else
            {
                var result = _engine.StartService( typeof( IConfigurationManagerService ).FullName );
                if( result.Success == false )
                {
                    Debugger.Break();
                    MessageBox.Show( "Could not start configuration manager.", "Failed to start configuration manager", MessageBoxButton.OK, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None );
                }
            }
        }

    }
}
