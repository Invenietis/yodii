using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    /// <summary>
    /// Simple serializable POCO that describes a configuration layer.
    /// </summary>
    [Serializable]
    public sealed class YodiiConfigurationLayer
    {
        string _name;
        readonly List<YodiiConfigurationItem> _items;

        /// <summary>
        /// Initializes a new layer.
        /// </summary>
        public YodiiConfigurationLayer()
        {
            _name = String.Empty;
            _items = new List<YodiiConfigurationItem>();
        }

        /// <summary>
        /// Gets or sets the name of this layer.
        /// Defaults to <see cref="String.Empty"/> that is the name of the default layer.
        /// </summary>
        public string Name 
        {
            get { return _name; }
            set { _name = value ?? String.Empty; } 
        }

        /// <summary>
        /// Gets whether this is the default layer: its <see cref="Name"/> is <see cref="String.Empty"/>.
        /// </summary>
        public bool IsDefault
        {
            get { return _name.Length == 0; }
        }

        /// <summary>
        /// Gets the configuration items that this configuration contains.
        /// </summary>
        public IList<YodiiConfigurationItem> Items
        {
            get { return _items; }
        }

    }
}
