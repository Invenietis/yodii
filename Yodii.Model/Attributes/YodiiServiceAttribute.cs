using System;

namespace Yodii.Model
{
    /// <summary>
    /// Informational attribute for Yodii services.
    /// Not necessary to create a plugin or service, but can be used to provide information
    /// in utilities providing information on Yodii itself, like the ObjectExplorer.
    /// </summary>
    [AttributeUsage( AttributeTargets.Interface, Inherited = false, AllowMultiple = false )]
    public class YodiiServiceAttribute : YodiiElementBaseAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiServiceAttribute"/> class.
        /// </summary>
        public YodiiServiceAttribute()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiServiceAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name of the service.</param>
        public YodiiServiceAttribute( string displayName )
        {
            DisplayName = displayName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="YodiiServiceAttribute"/> class.
        /// </summary>
        /// <param name="displayName">The display name of the service.</param>
        /// <param name="description">The description of the service.</param>
        public YodiiServiceAttribute( string displayName, string description )
            : this( displayName )
        {
            Description = description;
        }
    }
}