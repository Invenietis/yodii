using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using Yodii.Model;

namespace Yodii.Engine
{
    class ConfigurationItem : IConfigurationItem
    {
        readonly string _serviceOrPluginFullName;
        readonly ConfigurationLayer _owner;
        
        ConfigurationStatus _status;
        StartDependencyImpact _impact;
        string _statusReason;

        internal ConfigurationItem( ConfigurationLayer configurationLayer, string serviceOrPluginFullName, ConfigurationStatus initialStatus, StartDependencyImpact initialImpact, string initialStatusReason )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginFullName ) );
            Debug.Assert( configurationLayer != null );
            Debug.Assert( initialStatusReason != null );

            _owner = configurationLayer;
            _serviceOrPluginFullName = serviceOrPluginFullName;
            _status = initialStatus;
            _impact = initialImpact;
            _statusReason = initialStatusReason;
        }

        public string ServiceOrPluginFullName
        {
            get { return _serviceOrPluginFullName; }
        }

        public IConfigurationLayer Layer
        {
            get { return _statusReason == null ? null : _owner; }
        }

        public ConfigurationStatus Status
        {
            get { return _status; }
        }
        public StartDependencyImpact Impact
        {
            get { return _impact; }
        }

        /// <summary>
        /// Gets or sets an optional description for this configuration.
        /// This is null when <see cref="Layer"/> is null (this item does no more belong to its layer).
        /// </summary>
        public string Description
        {
            get { return _statusReason; }
            set
            {
                if ( _statusReason == null ) throw new InvalidOperationException();
                if( value == null ) value = String.Empty;
                if( _statusReason != value )
                {
                    _statusReason = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public IYodiiEngineResult Set( ConfigurationStatus newStatus, string newDescription = null )
        {
            return Set( newStatus, _impact, newDescription );
        }

        public IYodiiEngineResult Set( StartDependencyImpact newImpact, string newDescription = null )
        {
            return Set( _status, newImpact, newDescription );
        }

        public IYodiiEngineResult Set( ConfigurationStatus newStatus, StartDependencyImpact newImpact, string newDescription = null )
        {
            if( _statusReason == null ) throw new InvalidOperationException();
            var c = _owner.Items.ConfigurationManager;
            bool changeStatus = _status != newStatus;
            bool changeImpact = _impact != newImpact;
            if( !changeImpact && !changeStatus )
            {
                if( newDescription != null ) Description = newDescription;
                return _owner.Items.ConfigurationManager.Engine.SuccessResult;
            }
            IYodiiEngineResult result = c != null 
                                            ? c.OnConfigurationItemChanging( this, new FinalConfigurationItem( _serviceOrPluginFullName, newStatus, newImpact ) ) 
                                            : SuccessYodiiEngineResult.NullEngineSuccessResult;
            if( result.Success )
            {
                _impact = newImpact;
                _status = newStatus;
                if( newDescription != null ) Description = newDescription;
                if( changeImpact ) NotifyPropertyChanged( "Impact" );
                if( changeStatus ) NotifyPropertyChanged( "Status" );
                if( c != null ) c.OnConfigurationChanged();
            }
            return result;
        }

        internal void OnRemoved()
        {
            Debug.Assert( _statusReason != null );
            _statusReason = null;
            _status = ConfigurationStatus.Optional;
            _impact = StartDependencyImpact.Unknown;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName]string propertyName = "" )
        {
            var h = PropertyChanged;
            if( h != null ) h( this, new PropertyChangedEventArgs( propertyName ) );
        }

        #endregion INotifyPropertyChanged 
    }
}
