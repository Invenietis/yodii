using System;
using CK.Core;
namespace Yodii.Model
{
    /// <summary>
    /// Collection of configuration items.
    /// <seealso cref="IConfigurationLayer"/>
    /// </summary>
    public interface IConfigurationItemCollection : ICKObservableReadOnlyList<IConfigurationItem>
    {
        /// <summary>
        /// Attempts to add a configuration item.
        /// </summary>
        /// <param name="serviceOrPluginId">Service or plugin ID.</param>
        /// <param name="status">Required configuration status.</param>
        /// <param name="statusReason">Description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Add( string serviceOrPluginId, ConfigurationStatus status, string statusReason = "" );

        /// <summary>
        /// Attempts to remove a configuration item, effectively making it Optional.
        /// </summary>
        /// <param name="serviceOrPluginId">Service or plugin ID to remove.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Remove( string serviceOrPluginId );

        /// <summary>
        /// Accesses an item of this collection.
        /// </summary>
        /// <param name="key">ServiceOrPluginId</param>
        /// <returns>Configuration item.</returns>
        IConfigurationItem this[string key] { get; }
    }
}
