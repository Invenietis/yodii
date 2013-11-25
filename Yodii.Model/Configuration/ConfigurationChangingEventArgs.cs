using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Model
{
    public class ConfigurationChangingEventArgs : EventArgs
    {
        readonly IFinalConfiguration _finalConfiguration;
        readonly FinalConfigurationChange _finalConfigurationChange;
        readonly IConfigurationItem _configurationItemChanged;
        readonly IConfigurationLayer _configurationLayerChanged;

        List<string> _externalReasons;

        public IFinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        public FinalConfigurationChange FinalConfigurationChange
        {
            get { return _finalConfigurationChange; }
        }

        public IConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        public IConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        public bool IsCanceled
        {
            get { return _externalReasons != null; }
        }

        public IReadOnlyList<string> FailureExternalReasons
        {
            get { return _externalReasons != null ? _externalReasons.AsReadOnlyList() : CKReadOnlyListEmpty<string>.Empty; }
        }

        public ConfigurationChangingEventArgs( IFinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        public ConfigurationChangingEventArgs( IFinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationLayer configurationLayer )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }

        public void CancelForExternalReason( string reason )
        {
            if( String.IsNullOrWhiteSpace( reason ) ) throw new ArgumentException();
            if( _externalReasons == null ) _externalReasons = new List<string>();
            _externalReasons.Add( reason );
        }
    }
}
