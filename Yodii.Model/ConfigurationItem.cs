using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    public class ConfigurationItem : IConfigurationItem, INotifyPropertyChanged
    {
        private string _serviceOrPluginName;
        private ConfigurationStatus _status;
        private string _statusReason;
        private readonly ConfigurationLayer _configurationLayerParent;

        public string ServiceOrPluginName
        {
            get { return _serviceOrPluginName; }
        }

        public ConfigurationStatus Status
        {
            get { return _status; }
            internal set
            {
                _status = value;
                NotifyPropertyChanged();
            }
        }

        public string StatusReason
        {
            get { return _statusReason; }
            set 
            {
                if( value == null ) throw new NullReferenceException();
                _statusReason = value;
                NotifyPropertyChanged();
            }
        }

        internal ConfigurationItem( string serviceOrPluginName, ConfigurationStatus status, ConfigurationLayer configurationLayer, string statusReason = "" )
        {
            if( string.IsNullOrEmpty( serviceOrPluginName ) ) throw new ArgumentException( "serviceOrPluginID is null or empty" );
            if( configurationLayer == null ) throw new ArgumentNullException( "configurationLayer" );
            if( statusReason == null ) throw new ArgumentNullException( "statusReason" );

            _statusReason = statusReason;
            _serviceOrPluginName = serviceOrPluginName;
            _status = status;
            _configurationLayerParent = configurationLayer;
        }

        public ConfigurationResult SetStatus( ConfigurationStatus newStatus, string statusReason = "" )
        {
            if( statusReason == null ) throw new ArgumentNullException( "statusReason" );

            if( CanChangeStatus( newStatus ) )
            {
                ConfigurationResult configurationResult = _configurationLayerParent.OnConfigurationItemChanging(FinalConfigurationChange.StatusChanged, this, newStatus );
                if( configurationResult )
                {
                    Status = newStatus;
                    StatusReason = statusReason;
                    _configurationLayerParent.OnConfigurationItemChanged( FinalConfigurationChange.StatusChanged, this, newStatus );
                    return configurationResult;
                }
                return configurationResult;
            }
            return new ConfigurationResult( String.Format( "{2}({0}) could not be changed in {1}", _status, newStatus, _serviceOrPluginName ) );
        }

        public bool CanChangeStatus(ConfigurationStatus newStatus)
        {
            return (_status == ConfigurationStatus.Runnable) ? newStatus == ConfigurationStatus.Running :
                _status != ConfigurationStatus.Disable && _status != ConfigurationStatus.Running;
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged( [CallerMemberName] String propertyName = "" )
        {
            if( PropertyChanged != null )
            {
                PropertyChanged( this, new PropertyChangedEventArgs( propertyName ) );
            }
        }

        #endregion INotifyPropertyChanged
    }
}
