#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Mock model\LabPluginInfo.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System.Diagnostics;
using System.Windows;
using System.Windows.Input;
using Yodii.Model;

namespace Yodii.Lab.Mocks
{
    /// <summary>
    /// Lab plugin. Wrapper class around a mock PluginInfo, binding a LivePluginInfo when the engine is started.
    /// </summary>
    [DebuggerDisplay( "Lab plugin: {PluginInfo.PluginFullName}" )]
    public class LabPluginInfo : ViewModelBase
    {
        readonly IYodiiEngineExternal _engine;
        readonly PluginInfo _pluginInfo;
        ILivePluginInfo _livePluginInfo;

        internal LabPluginInfo( IYodiiEngineExternal engine, PluginInfo pluginInfo )
        {
            Debug.Assert( engine != null && pluginInfo != null );
            _engine = engine;
            _pluginInfo = pluginInfo;

            StartPluginCommand = new RelayCommand( ExecuteStartPlugin, CanExecuteStartPlugin );
            StopPluginCommand = new RelayCommand( ExecuteStopPlugin, CanExecuteStopPlugin );
        }

        private bool CanExecuteStopPlugin( object obj )
        {
            return LivePluginInfo != null && LivePluginInfo.RunningStatus == RunningStatus.Running && LivePluginInfo.Capability.CanStop;
        }

        private void ExecuteStopPlugin( object obj )
        {
            if( !CanExecuteStopPlugin( obj ) ) return;
            var result = _engine.StopItem( LivePluginInfo );
            if( !result.Success )
            {
                MessageBox.Show( result.Describe() );
            }
        }

        private bool CanExecuteStartPlugin( object obj )
        {
            StartDependencyImpact impact = StartDependencyImpact.Unknown;
            if( obj != null && obj is StartDependencyImpact ) impact = (StartDependencyImpact)obj;
            return LivePluginInfo != null && LivePluginInfo.Capability.CanStartWith( impact );
        }

        private void ExecuteStartPlugin( object obj )
        {
            if( !CanExecuteStartPlugin( obj ) ) return;
            StartDependencyImpact impact = StartDependencyImpact.Unknown;
            if( obj != null && obj is StartDependencyImpact ) impact = (StartDependencyImpact)obj;
            var result = _engine.StartItem( LivePluginInfo, impact );
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
                Debug.Assert( value == null || value.PluginInfo == PluginInfo );
                if( value != _livePluginInfo )
                {
                    _livePluginInfo = value;
                    RaisePropertyChanged();
                    RaisePropertyChanged( "IsLive" );
                }
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
