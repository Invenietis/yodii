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
