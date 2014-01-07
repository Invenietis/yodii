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
    public class ConfigurationItem : IConfigurationItem
    {
        readonly string _serviceOrPluginFullName;
        readonly ConfigurationLayer _owner;
        
        ConfigurationStatus _status;
        StartDependencyImpact _impact;
        string _statusReason;

        internal ConfigurationItem( ConfigurationLayer configurationLayer, string serviceOrPluginFullName, ConfigurationStatus initialStatus, StartDependencyImpact initialImpact, string initialStatusReason = "")
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
        /// Gets or sets an optional reason for this configuration status.
        /// Null when <see cref="Layer"/> is null (this item does no more belong to its layer).
        /// </summary>
        public string StatusReason
        {
            get { return _statusReason; }
            set
            {
                if ( _statusReason == null ) throw new InvalidOperationException();
                _statusReason = value ?? String.Empty;
                NotifyPropertyChanged();
            }
        }

        public IYodiiEngineResult SetStatus( ConfigurationStatus newStatus, string statusReason = "" )
        {
            if( _statusReason == null ) throw new InvalidOperationException();
            IYodiiEngineResult result = _owner.OnConfigurationItemChanging( this, new FinalConfigurationItem(_serviceOrPluginFullName, _status, _impact ) ); 
            if( result.Success )
            {
                _status = newStatus;
                NotifyPropertyChanged( "Status" );
                if( StatusReason != statusReason ) StatusReason = statusReason;
                if( _owner.ConfigurationManager != null ) _owner.ConfigurationManager.OnConfigurationChanged();
            }
            return result;
        }

        public IYodiiEngineResult SetImpact( StartDependencyImpact newImpact )
        {
            IYodiiEngineResult result = _owner.OnConfigurationItemChanging( this, new FinalConfigurationItem( _serviceOrPluginFullName, _status, _impact ) );
            if( result.Success )
            {
                _impact = newImpact;
                NotifyPropertyChanged( "Impact" );
                if( _owner.ConfigurationManager != null ) _owner.ConfigurationManager.OnConfigurationChanged();
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
