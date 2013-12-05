using System;
using System.ComponentModel;
namespace Yodii.Model
{
    /// <summary>
    /// Configuration manager interface. Contains a collection of <see cref="IConfigurationLayer"/>,
    /// each having a collection of <see cref="IConfigurationItem"/>.
    /// Adding and removing layers triggers configuration resolution, and formats everything into a single <see cref="FinalConfiguration"/>,
    /// which is essentially a single, read-only <see cref="IConfigurationLayer"/>.
    /// </summary>
    public interface IConfigurationManager : INotifyPropertyChanged
    {
        /// <summary>
        /// Triggered when a configuration change was not cancelled, once a new FinalConfiguration is available.
        /// </summary>
        event EventHandler<ConfigurationChangedEventArgs> ConfigurationChanged;

        /// <summary>
        /// Triggered during a configuration change, and permits cancellation. Contains a temporary FinalConfiguration.
        /// </summary>
        event EventHandler<ConfigurationChangingEventArgs> ConfigurationChanging;

        /// <summary>
        /// Read-only collection container of read-only configuration items.
        /// </summary>
        /// <remarks>
        /// Changes when an engine change was not cancelled through <see cref="IConfigurationManager.ConfigurationChanging"/>.
        /// </remarks>
        FinalConfiguration FinalConfiguration { get; }

        /// <summary>
        /// Layers contained in this manager.
        /// </summary>
        IConfigurationLayerCollection Layers { get; }
    }
}
