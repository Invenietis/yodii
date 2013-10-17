using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public class ConfigurationItem : INotifyPropertyChanged
    {
        readonly string _serviceOrPluginId;
        readonly ConfigurationLayer _owner;
        
        ConfigurationStatus _status;
        string _statusReason;

        internal ConfigurationItem( ConfigurationLayer configurationLayer, string serviceOrPluginId, ConfigurationStatus initialStatus, string initialStatusReason = "" )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceOrPluginId ) );
            Debug.Assert( configurationLayer != null );
            Debug.Assert( initialStatusReason != null );
            _owner = configurationLayer;
            _serviceOrPluginId = serviceOrPluginId;
            _status = initialStatus;
            _statusReason = initialStatusReason;
        }

        public string ServiceOrPluginId
        {
            get { return _serviceOrPluginId; }
        }

        public ConfigurationLayer Layer
        {
            get { return _statusReason == null ? null : _owner; }
        }

        public ConfigurationStatus Status
        {
            get { return _status; }
        }

        /// <summary>
        /// Gets or sets an optional reason for this configuration.
        /// Null when <see cref="Layer"/> is null (this item does no more belong to its layer).
        /// </summary>
        public string StatusReason
        {
            get { return _statusReason; }
            set
            {
                if( _statusReason == null ) throw new InvalidOperationException();
                _statusReason = value ?? String.Empty;
                NotifyPropertyChanged();
            }
        }

        public ConfigurationResult SetStatus( ConfigurationStatus newStatus )
        {
            if( _statusReason == null ) throw new InvalidOperationException();
            ConfigurationResult result = _owner.OnConfigurationItemChanging( this, newStatus ); 
            if( result )
            {
                _status = newStatus;
                NotifyPropertyChanged( "Status" );
                if( _owner.ConfigurationManager != null ) _owner.ConfigurationManager.OnConfigurationChanged();
                return result;
            }
            return result;
        }

        internal void OnRemoved()
        {
            Debug.Assert( _statusReason != null );
            _statusReason = null;
            _status = ConfigurationStatus.Optional;
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
