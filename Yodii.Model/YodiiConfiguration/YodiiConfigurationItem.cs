using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Configuration for an item identified by its <see cref="ServiceOrPluginFullName"/>.
    /// This is a simple serializable POCO.
    /// </summary>
    [Serializable]
    public sealed class YodiiConfigurationItem : IConfigurationItemData
    {
        string _name;
        string _description;

        /// <summary>
        /// Initializes a new item.
        /// </summary>
        public YodiiConfigurationItem()
        {
            _name = _description = String.Empty;
        }

        /// <summary>
        /// Gets or sets the service or plugin full name.
        /// Defaults to <see cref="String.Empty"/>.
        /// </summary>
        public string ServiceOrPluginFullName 
        {
            get { return _name; }
            set { _name = value ?? String.Empty; } 
        }

        /// <summary>
        /// Gets or sets the <see cref="ConfigurationStatus"/> for this <see cref="ServiceOrPluginFullName"/>.
        /// Defaults to <see cref="ConfigurationStatus.Optional"/>.
        /// </summary>
        public ConfigurationStatus Status { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="StartDependencyImpact"/> for this <see cref="ServiceOrPluginFullName"/>.
        /// Defaults to <see cref="StartDependencyImpact.Unknown"/>.
        /// </summary>
        public StartDependencyImpact Impact { get; set; }

        /// <summary>
        /// Gets or sets an optional description for this configuration item.
        /// Defaults to <see cref="String.Empty"/>.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value ?? String.Empty; }
        }

    }
}
