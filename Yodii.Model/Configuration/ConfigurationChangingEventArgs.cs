using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Model
{
    /// <summary>
    /// Details of a ConfigurationChanging event.
    /// </summary>
    public class ConfigurationChangingEventArgs : EventArgs
    {
        readonly FinalConfiguration _finalConfiguration;
        readonly FinalConfigurationChange _finalConfigurationChange;
        readonly IConfigurationItem _configurationItemChanged;
        readonly IConfigurationLayer _configurationLayerChanged;

        List<string> _externalReasons;

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
        public FinalConfigurationChange FinalConfigurationChange
        {
            get { return _finalConfigurationChange; }
        }

        /// <summary>
        /// Changed item.
        /// </summary>
        public IConfigurationItem ConfigurationItemChanged
        {
            get { return _configurationItemChanged; }
        }

        /// <summary>
        /// Changed layer.
        /// </summary>
        public IConfigurationLayer ConfigurationLayerChanged
        {
            get { return _configurationLayerChanged; }
        }

        /// <summary>
        /// Whether the change was canceled.
        /// </summary>
        public bool IsCanceled
        {
            get { return _externalReasons != null; }
        }

        /// <summary>
        /// List of reasons of why the change was canceled.
        /// </summary>
        public IReadOnlyList<string> FailureExternalReasons
        {
            get { return _externalReasons != null ? _externalReasons.AsReadOnlyList() : CKReadOnlyListEmpty<string>.Empty; }
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangingEventArgs for a <see cref="IConfigurationLayerCollection.Clear"/>.
        /// </summary>
        /// <param name="finalConfiguration">New empty FinalConfiguration. Must be empty otherwise an exception is thrown.</param>
        public ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration )
        {
            if( finalConfiguration == null || finalConfiguration.Items.Count > 0 ) throw new ArgumentException( "Must be not null and empty.", "finalConfiguration" );
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = FinalConfigurationChange.Cleared;
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangingEventArgs when change is triggered by a ConfigurationItem.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Details of changes in the new FinalConfiguration</param>
        /// <param name="configurationItem">Item that provoked this change</param>
        public ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationItem configurationItem )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationItemChanged = configurationItem;
        }

        /// <summary>
        /// Creates a new instance of ConfigurationChangingEventArgs when change is triggered by a ConfigurationLayer.
        /// </summary>
        /// <param name="finalConfiguration">New FinalConfiguration</param>
        /// <param name="finalConfigurationChanged">Details of changes in the new FinalConfiguration</param>
        /// <param name="configurationLayer">Layer that provoked this change</param>
        public ConfigurationChangingEventArgs( FinalConfiguration finalConfiguration, FinalConfigurationChange finalConfigurationChanged, IConfigurationLayer configurationLayer )
        {
            _finalConfiguration = finalConfiguration;
            _finalConfigurationChange = finalConfigurationChanged;
            _configurationLayerChanged = configurationLayer;
        }

        /// <summary>
        /// Cancel this change with a reason.
        /// </summary>
        /// <param name="reason">Why this change is canceled.</param>
        public void CancelForExternalReason( string reason )
        {
            if( String.IsNullOrWhiteSpace( reason ) ) throw new ArgumentException();
            if( _externalReasons == null ) _externalReasons = new List<string>();
            _externalReasons.Add( reason );
        }
    }
}
