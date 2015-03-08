using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Discoverer.Tests
{
    [TestFixture]
    public class DiscoveredInfoTests
    {
        [Test]
        public void DiscoveredInfo_implementation_is_serializable()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            var copy = SerializationCopy( new YodiiConfiguration() { DiscoveredInfo = discoverer.GetDiscoveredInfo() } );
            DiscovererTests.CheckPlugin1AndPlugin1Bis( copy.DiscoveredInfo );
        }

        static T SerializationCopy<T>( T o )
        {
            using( var s = new MemoryStream() )
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize( s, o );
                s.Position = 0;
                return (T)f.Deserialize( s );
            }
        }


    }
}
