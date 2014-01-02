using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CK.Core;

namespace Yodii.Lab
{
    /// <summary>
    /// Recent file. Used in ribbon backstage and stored in Application settings.
    /// </summary>
    [Serializable]
    public class RecentFile
    {
        readonly FileInfo _file;
        readonly DateTime _accessTime;

        /// <summary>
        /// Initializes a new recent file.
        /// </summary>
        /// <param name="info">The file information.</param>
        /// <param name="accessTime">The last access time from this application. Must be <see cref="DateTimeKind.Utc"/> otherwise an exception is thrown.</param>
        public RecentFile( FileInfo info, DateTime accessTime )
        {
            if( info == null ) throw new ArgumentNullException( "info" );
            if( accessTime.Kind != DateTimeKind.Utc ) throw new ArgumentException( "Must be Utc.", "accessTime" );
            _file = info;
            _accessTime = accessTime;
        }

        /// <summary>
        /// Tries to parse a recent file name entry.
        /// </summary>
        /// <param name="monitor">Monitor that may receive errors.</param>
        /// <param name="s">String to parse.</param>
        /// <returns>A recent file or null if parsing failed.</returns>
        public static RecentFile TryParse( IActivityMonitor monitor, string s )
        {
            if( monitor == null ) throw new ArgumentNullException( "monitor" );
            if( s == null ) throw new ArgumentNullException( "s" );
            int pipeIdx = s.IndexOf( '|' );
            if( pipeIdx > 0 )
            {
                string fName = s.Substring( 0, pipeIdx );
                if( System.IO.File.Exists( fName ) )
                {
                    DateTime accessTime;
                    if( !FileUtil.TryParseFileNameUniqueTimeUtcFormat( s.Substring( pipeIdx + 1 ), out accessTime ) )
                    {
                        monitor.Warn().Send( "Invalid recent file access time for '{0}'. It is set to now.", fName );
                        accessTime = DateTime.UtcNow;
                    }
                    return new RecentFile( new FileInfo( fName ), accessTime );
                }
                else monitor.Warn().Send( "Recent file '{0}' does not exist. It is ignored.", fName );
            }
            else monitor.Warn().Send( "Invalid recent file entry '{0}'. It is ignored.", s );
            return null;
        }

        /// <summary>
        /// Gets the file name only part.
        /// </summary>
        public string FileName { get { return _file.Name; } }

        /// <summary>
        /// Gets the directory of the file.
        /// </summary>
        public string Directory { get { return _file.DirectoryName; } }

        /// <summary>
        /// Gets the last access time (necessarily in <see cref="DateTimeKind.Utc"/>).
        /// </summary>
        public DateTime AccessTime { get { return _accessTime; } }

        /// <summary>
        /// Gets the file information.
        /// </summary>
        public FileInfo File { get { return _file; } }

        /// <summary>
        /// Overridden to return a string that can be parsed with <see cref="TryParse"/>.
        /// </summary>
        /// <returns>Formatted string.</returns>
        public override string ToString()
        {
            return _file.FullName + '|' + _accessTime.ToString( FileUtil.FileNameUniqueTimeUtcFormat );
        }
    }
}
