using System;
using System.ComponentModel;
namespace Yodii.Model
{
    /// <summary>
    /// Configuration layer, containing configuration items.
    /// <see cref="IConfigurationManager"/>
    /// </summary>
    public interface IConfigurationLayer : INotifyPropertyChanged
    {
        /// <summary>
        /// Parent <see cref="IConfigurationManager"/>.
        /// </summary>
        IConfigurationManager ConfigurationManager { get; }

        /// <summary>
        /// Collection of IConfigurationItems in this layer.
        /// </summary>
        IConfigurationItemCollection Items { get; }

        /// <summary>
        /// Display name of the layer.
        /// </summary>
        string LayerName { get; set; }
    }
}
