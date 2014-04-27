using CK.Core;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Yodii.Model
{
    [AttributeUsage( AttributeTargets.Class, AllowMultiple = false, Inherited = false )]
    public sealed class PluginAttribute : Attribute
    {
        Guid _pluginId;
        string _description;
        Version _version;
        string[] _categories;

        /// <summary>
        /// Initializes a new <see cref="PluginAttribute"/> with its <see cref="Id"/>.
        /// </summary>
        /// <param name="pluginIdentifier">Identifier of the plugin.</param>
        public PluginAttribute( string pluginIdentifier )
        {
            _pluginId = new Guid( pluginIdentifier );
            _description = string.Empty;
            PublicName = GetType().Name;
            _version = Util.EmptyVersion;
        }

        /// <summary>
        /// Gets the unique identifier of the plugin.
        /// </summary>
        public Guid Id
        {
            get { return _pluginId; }
        }

        /// <summary>
        /// Gets or sets the public name of the plugin. Can be any string in any culture.
        /// </summary>
        public string PublicName { get; set; }

        /// <summary>
        /// Gets or sets the description of the plugin.
        /// </summary>
        public string Description
        {
            get { return _description; }
            set { _description = value != null ? value : String.Empty; }
        }

        /// <summary>
        /// Gets or sets the <see cref="System.Version"/> of the plugin. It is a string
        /// with at least the first two of the "Major.Minor.Build.Revision" standard version.
        /// Defaults to <see cref="Util.EmptyVersion"/>.
        /// </summary>
        public string Version
        {
            get { return _version.ToString(); }
            set { _version = value != null ? new Version( value ) : Util.EmptyVersion; }
        }

        /// <summary>
        /// Gets or sets an optional url that describes the plugin. Can be null.
        /// </summary>
        public string RefUrl { get; set; }

        /// <summary>
        /// Gets or sets an optional url where we can find an Icon attached to the plugin. Can be null.
        /// </summary>
        public string IconUri { get; set; }

        /// <summary>
        /// Gets or sets an optional list of categories, used to sort the plugin list by theme.
        /// Can also be used to define if this plugin must appear in the "Public" or "Advanced" configuration panel.
        /// Will never be null (an empty array will be returned instead of null).
        /// </summary>
        public string[] Categories
        {
            get { return _categories ?? CK.Core.Util.EmptyStringArray; }
            set { _categories = value; }
        } 
    }
}
