using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yodii.Model
{
    /// <summary>
    /// Base class for Yodii service and plugin informational attributes.
    /// Not necessary to create a plugin or service, but can be used to provide information
    /// in utilities providing information on Yodii itself, like the ObjectExplorer.
    /// </summary>
    public abstract class YodiiItemBaseAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the display name of the plugin or service.
        /// </summary>
        /// <value>
        /// The display name of the plugin or service.
        /// </value>
        public string DisplayName { get; set; }

        /// <summary>
        /// Gets or sets the description of the plugin or service.
        /// </summary>
        /// <value>
        /// The description of the plugin or service.
        /// </value>
        public string Description { get; set; }
    }
}
