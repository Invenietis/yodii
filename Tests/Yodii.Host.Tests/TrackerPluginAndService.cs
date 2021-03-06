#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\TrackerPluginAndService.cs) is part of CiviKey. 
*  
* CiviKey is free software: you can redistribute it and/or modify 
* it under the terms of the GNU Lesser General Public License as published 
* by the Free Software Foundation, either version 3 of the License, or 
* (at your option) any later version. 
*  
* CiviKey is distributed in the hope that it will be useful, 
* but WITHOUT ANY WARRANTY; without even the implied warranty of
* MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the 
* GNU Lesser General Public License for more details. 
* You should have received a copy of the GNU Lesser General Public License 
* along with CiviKey.  If not, see <http://www.gnu.org/licenses/>. 
*  
* Copyright © 2007-2015, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Yodii.Model;
using CK.Core;
using System.Text.RegularExpressions;

namespace Yodii.Host.Tests
{

    /// <summary>
    /// Simple tracking Service whose plugin (TrackerPlugin) can always run.
    /// </summary>
    public interface ITrackerService : IYodiiService
    {
        void AddEntry( IYodiiPlugin p, string action );
        void Clear();
        IReadOnlyList<TrackedEntry> Entries { get; }
    }

    public class TrackedEntry
    {
        public readonly IYodiiPlugin Plugin;
        public readonly string Action;

        internal TrackedEntry( IYodiiPlugin p, string a )
        {
            Plugin = p;
            Action = a;
        }
    }
    
    public static class TrackerExtensions
    {
        /// <summary>
        /// Gets the <see cref="TrackerPlugin.Entries"/> from the actual plugin object.
        /// Null if the plugin is Disabled since TrackerPlugin is IDisposable (it is totally removed when Disabled).
        /// </summary>
        /// <param name="host">This host.</param>
        /// <returns>The current list.</returns>
        static public IReadOnlyList<TrackedEntry> GetTrackedEntries( this YodiiHost host, bool clearEnries = true )
        {
            var p = (TrackerPlugin)host.FindLoadedPlugin( "Yodii.Host.Tests.TrackerPlugin" ).RealPluginObject;
            if( p == null ) return null;
            TestHelper.ConsoleMonitor.Trace().Send( p.Entries.Stringify() );
            if( !clearEnries ) return p.Entries;
            var e = p.Entries.ToReadOnlyList();
            p.Clear();
            return e;
        }

        static public string Stringify( this IReadOnlyList<TrackedEntry> entries )
        {
            StringBuilder b = new StringBuilder();
            foreach( var s in entries )
            {
                b.Append( s.Plugin.GetType().Name ).Append( ':' ).AppendLine( s.Action );
            }
            return b.ToString();
        }

        static public bool IsMatch( this IReadOnlyList<TrackedEntry> entries, string actions )
        {
            actions = Regex.Escape( actions );
            actions = actions.Replace( @"\.\.\.", @".*" );
            actions = actions.Replace( @",", Environment.NewLine );
            return Regex.IsMatch( entries.Stringify(), actions, RegexOptions.Singleline );
        }

    }

    /// <summary>
    /// Plugin that implements ITrackerService. No dependencies.
    /// </summary>
    public class TrackerPlugin : YodiiPluginBase, ITrackerService
    {
        readonly List<TrackedEntry> _entries;

        public TrackerPlugin()
        {
            _entries = new List<TrackedEntry>();
        }

        public void AddEntry( IYodiiPlugin p, string action )
        {
            _entries.Add( new TrackedEntry( p, action ) );
        }

        public void Clear()
        {
            _entries.Clear();
        }

        public IReadOnlyList<TrackedEntry> Entries 
        { 
            get { return _entries.ToReadOnlyList(); } 
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            AddEntry( this, "PreStart" );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            AddEntry( this, "PreStop" );
        }
        
        protected override void PluginStart( IStartContext c )
        {
            AddEntry( this, "Start" );
        }

        protected override void PluginStop( IStopContext c )
        {
            AddEntry( this, "Stop" );
        }
    }

}
