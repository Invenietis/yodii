using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    public class ConfigurationChangedEventArgs : EventArgs
    {
        private FinalConfiguration _finalConfiguration;

        private FinalConfigurationChange _finalConfigurationChanged;
        private IConfigurationItem _configurationItemChanged;
        private IConfigurationLayer _configurationLayerChanged;

        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        public FinalConfigurationChange FinalConfigurationChanged
        {
            get { return _finalConfigurationChanged; }
        }

        public IConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        public IConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        public ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        public ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationLayer configurationLayer )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }
    }
}
