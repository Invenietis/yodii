using System;

namespace Yodii.Model
{
    /// <summary>
    /// Informational attribute for Yodii plugins.
    /// Not necessary to create a plugin or service, but can be used to provide information
    /// in utilities providing information on Yodii itself, like the ObjectExplorer.
    /// </summary>
    [AttributeUsage( AttributeTargets.Class, Inherited = false, AllowMultiple = false )]
    public class YodiiPluginAttribute : YodiiItemBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiPluginAttribute"/> class.
        /// </summary>
        public YodiiPluginAttribute()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiPluginAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name of the plugin.</param>
        public YodiiPluginAttribute( string displayName )
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiPluginAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name of the plugin.</param>
        /// <param name="description">The description of the plugin.</param>
        public YodiiPluginAttribute( string displayName, string description )
            : this( displayName )
        {
            Description = description;
        }
    }
}