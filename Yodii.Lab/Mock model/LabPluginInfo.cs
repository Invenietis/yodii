using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    /// <summary>
    /// Lab plugin. Wrapper class around a mock PluginInfo, binding a LivePluginInfo when the engine is started.
    /// </summary>
    [DebuggerDisplay( "Lab {PluginInfo.PluginFullName} = {PluginInfo.PluginId}" )]
    public class LabPluginInfo : ViewModelBase
    {
        readonly PluginInfo _pluginInfo;
        ILivePluginInfo _livePluginInfo;

        internal LabPluginInfo( PluginInfo pluginInfo )
        {
            Debug.Assert( pluginInfo != null );

            _pluginInfo = pluginInfo;

            StartPluginCommand = new RelayCommand( ExecuteStartPlugin, CanExecuteStartPlugin );
            StopPluginCommand = new RelayCommand( ExecuteStopPlugin, CanExecuteStopPlugin );
        }

        private bool CanExecuteStopPlugin( object obj )
        {
            return LivePluginInfo != null && LivePluginInfo.RunningStatus == RunningStatus.Running;
        }

        private void ExecuteStopPlugin( object obj )
        {
            if( !CanExecuteStopPlugin( null ) ) return;
            var result = LivePluginInfo.Stop( "LabPluginInfo" );
            if( !result.Success )
            {
                MessageBox.Show( result.Describe() );
            }
        }

        private bool CanExecuteStartPlugin( object obj )
        {
            return LivePluginInfo != null && LivePluginInfo.RunningStatus == RunningStatus.Stopped;
        }

        private void ExecuteStartPlugin( object obj )
        {
            if( !CanExecuteStartPlugin( null ) ) return;
            var result = LivePluginInfo.Start( "LabPluginInfo" );
            if( !result.Success )
            {
                MessageBox.Show( result.Describe() );
            }
        }

        #region Properties

        /// <summary>
        /// Command to start this plugin.
        /// </summary>
        public ICommand StartPluginCommand { get; private set; }

        /// <summary>
        /// Command to stop this plugin.
        /// </summary>
        public ICommand StopPluginCommand { get; private set; }

        /// <summary>
        /// Attached PluginInfo. Read-only.
        /// </summary>
        public PluginInfo PluginInfo
        {
            get { return _pluginInfo; }
        }

        /// <summary>
        /// Active LivePluginInfo attached to this lab.
        /// Null if the lab is in building mode, when the engine hasn't started.
        /// </summary>
        public ILivePluginInfo LivePluginInfo
        {
            get { return _livePluginInfo; }
            internal set
            {
                if( value != null )
                {
                    Debug.Assert( value.PluginInfo == PluginInfo );
                }

                _livePluginInfo = value;

                RaisePropertyChanged();
                RaisePropertyChanged( "IsLive" );
            }
        }

        /// <summary>
        /// True if the lab is in simulation mode, and this LabPluginInfo has a LivePluginInfo.
        /// False if the lab is in building mode.
        /// </summary>
        public bool IsLive
        {
            get
            {
                return LivePluginInfo != null;
            }
        }
        #endregion Properties
    }
}
