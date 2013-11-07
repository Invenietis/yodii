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
        #endregion

        #region Constructors
        internal YodiiGraphVertex( LivePluginInfo plugin )
        {
            _isPlugin = true;
            _livePlugin = plugin;
        }

        internal YodiiGraphVertex( LiveServiceInfo service )
        {
            _isPlugin = false;
            _liveService = service;
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
    }
}
