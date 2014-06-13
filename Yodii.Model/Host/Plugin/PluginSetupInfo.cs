using System;

namespace Yodii.Model
{
    public class PluginSetupInfo
    {
        /// <summary>
        /// Gets or sets an explicit message for the user when <see cref="IPlugin.Setup"/> fails.
        /// </summary>
        public string FailedUserMessage { get; set; }

        /// <summary>
        /// Gets or sets a message for the user when <see cref="IPlugin.Setup"/> fails.
        /// </summary>
        public string FailedDetailedMessage { get; set; }

        /// <summary>
        /// Gets or sets an optional exception.
        /// </summary>
        public Exception Error { get; set; }

        /// <summary>
        /// Clears <see cref="FailedUserMessage"/>, <see cref="FailedDetailedMessage"/> and <see cref="Error"/>: they are all set to null.
        /// </summary>
        public void Clear()
        {
            FailedUserMessage = null;
            FailedDetailedMessage = null;
            Error = null;
        }
    }
}
