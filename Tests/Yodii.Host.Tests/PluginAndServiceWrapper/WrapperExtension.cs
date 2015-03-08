#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\PluginAndServiceWrapper\WrapperExtension.cs) is part of CiviKey. 
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
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    static class WrapperExtension
    {
        public static void CheckState( this IEnumerable<PluginWrapper> @this, PluginStatus s )
        {
            @this.Select( p => p.CheckState( s ) ).LastOrDefault();
        }

        public static void CheckState( this IEnumerable<ServiceWrapper> @this, ServiceStatus s )
        {
            @this.Select( p => p.CheckState( s ) ).LastOrDefault();
        }

        public static void ClearEvents( this IEnumerable<ServiceWrapper> @this )
        {
            foreach( var s in @this ) s.ClearEvents();
        }

        public static void CheckEvents( this IEnumerable<ServiceStatus> @this, params ServiceStatus[] statuses )
        {
            CollectionAssert.AreEqual( statuses, @this );
        }

        public static IEnumerable<Tuple<ServiceWrapper,ServiceStatus>> AllEvents( this IEnumerable<ServiceWrapper> @this )
        {
            return @this.SelectMany( s => s.Events.Select( e => Tuple.Create( s, e ) ) );
        }
    }
}
