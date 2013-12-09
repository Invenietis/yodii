using System.Diagnostics;
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
        }

        #region Properties

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

                    _livePluginInfo = value;
                }

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
