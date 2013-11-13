using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Lab.Mocks;
using Yodii.Model;

namespace Yodii.Lab
{
    public class YodiiGraphVertex : ViewModelBase
    {
        #region Fields
        readonly bool _isPlugin;
        readonly LiveServiceInfo _liveService;
        readonly LivePluginInfo _livePlugin;
        bool _isSelected = false;
        ConfigurationStatus _configStatus;
        YodiiGraph _parentGraph;
        bool _hasConfiguration;
        #endregion

        #region Constructors
        internal YodiiGraphVertex( YodiiGraph parentGraph, LivePluginInfo plugin )
        {
            _isPlugin = true;
            _livePlugin = plugin;
            _parentGraph = parentGraph;

            _livePlugin.PluginInfo.PropertyChanged += Info_PropertyChanged;
        }

        void Info_PropertyChanged( object sender, System.ComponentModel.PropertyChangedEventArgs e )
        {
            RaisePropertyChanged( "Title" );
        }

        internal YodiiGraphVertex( LiveServiceInfo service )
        {
            _isPlugin = false;
            _liveService = service;

            _liveService.ServiceInfo.PropertyChanged += Info_PropertyChanged;
        }
        #endregion Constructors

        #region Properties
        public bool IsPlugin { get { return _isPlugin; } }
        public bool IsService { get { return !_isPlugin; } }

        public bool IsSelected
        {
            get { return _isSelected; }
            internal set
            {
                if( value != _isSelected )
                {
                    _isSelected = value;
                    RaisePropertyChanged( "IsSelected" );
                }
            }
        }

        public bool HasConfiguration
        {
            get { return _hasConfiguration; }
            internal set
            {
                if( value != _hasConfiguration )
                {
                    _hasConfiguration = value;
                    RaisePropertyChanged( "HasConfiguration" );
                }
            }
        }

        public ConfigurationStatus ConfigurationStatus
        {
            get { return _configStatus; }
            internal set
            {
                if( value != _configStatus )
                {
                    _configStatus = value;
                    RaisePropertyChanged( "ConfigurationStatus" );
                }
            }
        }

        // Global view properties
        public string Title
        {
            get
            {
                if( IsService )
                    return LiveServiceInfo.ServiceInfo.ServiceFullName;
                else
                    return LivePluginInfo.PluginInfo.PluginFullName;
            }
        }
        public LiveServiceInfo LiveServiceInfo { get { return _liveService; } }
        public LivePluginInfo LivePluginInfo { get { return _livePlugin; } }

        #endregion Properties

        internal void RaiseStatusChange()
        {
            RaisePropertyChanged( "ConfigurationStatus" );
            RaisePropertyChanged( "HasConfiguration" );
        }

        internal void RemoveSelf()
        {
            if( IsService )
            {
                _parentGraph.RemoveService( LiveServiceInfo.ServiceInfo );
            } else if (IsPlugin)
            {
                _parentGraph.RemovePlugin( LivePluginInfo.PluginInfo );
            }
        }
    }
}
