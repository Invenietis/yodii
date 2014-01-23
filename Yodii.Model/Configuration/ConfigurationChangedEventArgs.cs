using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Details concerning a change in the Configurationmanager.
    /// </summary>
    public class ConfigurationChangedEventArgs : EventArgs
    {
        private FinalConfiguration _finalConfiguration;

        private FinalConfigurationChange _finalConfigurationChanged;
        private IConfigurationItem _configurationItemChanged;
        private IConfigurationLayer _configurationLayerChanged;

        /// <summary>
        /// New FinalConfiguration.
        /// </summary>
        public FinalConfiguration FinalConfiguration
        {
            get { return _finalConfiguration; }
        }

        /// <summary>
        /// Details of changes in the FinalConfiguration.
        /// </summary>
        public FinalConfigurationChange FinalConfigurationChanged
        {
            get { return _finalConfigurationChanged; }
        }

        /// <summary>
        /// The ConfigurationItem that changed.
        /// </summary>
        public IConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        /// <summary>
        /// The ConfigurationLayer that changed.
        /// </summary>
        public IConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangedEventArgs provoked by a ConfigurationItem.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Changes of the new FinalConfiguration</param>
        /// <param name="configurationItem">Item that changed</param>
        public ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangedEventArgs provoked by a ConfigurationLayer.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Changes of the new FinalConfiguration</param>
        /// <param name="configurationLayer">Layer that changed</param>
        public ConfigurationChangedEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationLayer configurationLayer )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChanged = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }
    }
}
