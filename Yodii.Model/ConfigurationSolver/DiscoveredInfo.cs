using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;

namespace Yodii.Model.ConfigurationSolver
{
    /// <summary>
    /// Base class for all discovered object.
    /// </summary>
    public abstract class DiscoveredInfo
    {
        /// <summary>
        /// Gets whether a non empty <see cref="ErrorMessage"/> exists.
        /// </summary>
        public bool HasError { get { return ErrorMessage != null; } }

        /// <summary>
        /// Gets the error message related to the discovering of the object.
        /// </summary>
        public string ErrorMessage { get; private set; }

        public void AddErrorLine( string message )
        {
            if ( String.IsNullOrEmpty( message ) ) throw new ArgumentNullException( message );
            if ( ErrorMessage == null ) ErrorMessage = message;
            else ErrorMessage += Environment.NewLine + message;
        }
    }
}
