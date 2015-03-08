#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\FailureTransitionPlugin\FailureTransitionPlugin.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    /// <summary>
    /// This plugin can fail its PreStart or PreStop calls depending on FailureTransitionPlugin.CancelPreStart and CancelPreStop static properties.
    /// </summary>
    public class FailureTransitionPluginDisposable : FailureTransitionPlugin, IDisposable
    {
        public FailureTransitionPluginDisposable( IAnotherService s, ITrackerService tracker ) : base( s, tracker ) {}
        public void Dispose() {}
    }

    /// <summary>
    /// This plugin can fail its PreStart or PreStop calls depending on CancelPreStart and CancelPreStop static properties.
    /// </summary>
    public class FailureTransitionPlugin : YodiiPluginBase, IFailureTransitionPluginService
    {
        readonly ITrackerService _tracker;
        readonly IAnotherService _another;

        public static bool CancelPreStart = false;
        public static bool CancelPreStop = false;

        public FailureTransitionPlugin( IAnotherService s, ITrackerService tracker )
        {
            _tracker = tracker;
            _another = s;
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.AddEntry( this, "PreStart" ) );
            if( CancelPreStart ) c.Cancel( "Canceled!" );
        }

        protected override void PluginStart( IStartContext c )
        {
            _tracker.AddEntry( this, "Start" );
            Assert.That( c.CancellingPreStop, Is.EqualTo( CancelPreStop ) );
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _tracker.AddEntry( this, "PreStop" );
            if( CancelPreStop ) c.Cancel( "Canceled!" );
        }

        protected override void PluginStop( IStopContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.AddEntry( this, "Stop" ) );
            Assert.That( c.CancellingPreStart, Is.EqualTo( CancelPreStart ) );
        }

        void IFailureTransitionPluginService.DoSomething()
        {
            _tracker.AddEntry( this, "DoSomething" );
        }
    }
}