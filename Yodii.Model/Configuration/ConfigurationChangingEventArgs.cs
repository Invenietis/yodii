using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;

namespace Yodii.Model
{
    public class ConfigurationChangingEventArgs : EventArgs
    {
        readonly FinalConfiguration _finalConfiguration;
        readonly FinalConfigurationChange _finalConfigurationChange;
        readonly ConfigurationItem _configurationItemChanged;
        readonly ConfigurationLayer _configurationLayerChanged;

        List<string> _externalReasons;
        IStaticFailureResult _staticFailure;
        IDynamicFailureResult _dynamicFailure;

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        public FinalConfigurationChange FinalConfigurationChange
        {
            get { return _finalConfigurationChange; }
        }

        public ConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        public ConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        public bool IsCanceled
        {
            get { return _externalReasons != null || _staticFailure != null || _dynamicFailure != null; }
        }

        public IReadOnlyList<string> FailureExternalReasons
        {
            get { return _externalReasons != null ? _externalReasons.AsReadOnlyList() : CKReadOnlyListEmpty<string>.Empty; }
        }

        public IStaticFailureResult StaticFailure
        {
            get { return _staticFailure; }
        }

        public IDynamicFailureResult DynamicFailure
        {
            get { return _dynamicFailure; }
        }




        internal ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        internal ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, ConfigurationLayer configurationLayer )
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

        public void CancelByStaticResolution( IStaticFailureResult failure )
        {
            if( failure == null || (failure.BlockingPlugins.Count == 0 && failure.BlockingServices.Count == 0) ) throw new ArgumentException(); 
            _isCanceled = true;
            _staticFailure = failure;
        }

        public void CancelByDynamicStep( IDynamicFailureResult failure )
        {
            if( failure == null || failure.ErrorPlugins.Count == 0 ) throw new ArgumentException();
            _isCanceled = true;
            _dynamicFailure = failure;
        }
    }
}
