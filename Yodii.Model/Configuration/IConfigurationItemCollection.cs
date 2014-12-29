using System;
using CK.Core;
namespace Yodii.Model
{
    /// <summary>
    /// Collection of configuration items hold by a <see cref="IConfigurationLayer"/>.
    /// </summary>
    public interface IConfigurationItemCollection : ICKObservableReadOnlyList<IConfigurationItem>
    {
        /// <summary>
        /// Attempts to add or set a configuration item with a status and an impact.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name.</param>
        /// <param name="status">Required configuration status.</param>
        /// <param name="impact">Required impact.</param>
        /// <param name="description">Optional description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Set( string serviceOrPluginFullName, ConfigurationStatus status, StartDependencyImpact impact, string description = null );

        /// <summary>
        /// Attempts to add or set a status on an item.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name.</param>
        /// <param name="status">Required configuration status.</param>
        /// <param name="description">Optional description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Set( string serviceOrPluginFullName, ConfigurationStatus status, string description = null );

        /// <summary>
        /// Attempts to add or set an impact on an item.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name.</param>
        /// <param name="impact">Required impact.</param>
        /// <param name="description">Optional description of the change.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Set( string serviceOrPluginFullName, StartDependencyImpact impact, string description = null );

        /// <summary>
        /// Attempts to remove a configuration item, effectively making it Optional.
        /// </summary>
        /// <param name="serviceOrPluginFullName">Service or plugin name to remove.</param>
        /// <returns>Yodii engine change result.</returns>
        IYodiiEngineResult Remove( string serviceOrPluginFullName );

        /// <summary>
        /// Gets the <see cref="IConfigurationItem"/> for the given plugin or service.
        /// </summary>
        /// <param name="key">Service or Plugin full name</param>
        /// <returns>Configuration item.</returns>
        IConfigurationItem this[string key] { get; }

        /// <summary>
        /// Gets the <see cref="IConfigurationLayer"/> that contains this collection.
        /// </summary>
        IConfigurationLayer ParentLayer { get; }
    }
}
