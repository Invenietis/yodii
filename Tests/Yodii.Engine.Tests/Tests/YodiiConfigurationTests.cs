using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using NUnit.Framework;
using Yodii.Engine.Tests.Mocks;
using Yodii.Model;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    public class YodiiConfigurationTests
    {
        [Test]
        public void serialization_with_null_discovered_info()
        {
            var c = CreateConfiguration();
            YodiiConfiguration copy = SerializeAndDeserialize( c );
            CheckConfiguration( copy );
            Assert.That( copy.DiscoveredInfo, Is.Null );
        }

        [Test]
        public void serialization_with_nonserializable_discovered_info()
        {
            var c = CreateConfiguration();
            c.DiscoveredInfo = MockInfoFactory.BigGraph();
            YodiiConfiguration copy = SerializeAndDeserialize( c );
            CheckConfiguration( copy );
            Assert.That( copy.DiscoveredInfo, Is.Null );
        }

        [Serializable]
        class SerializableDiscoveredInfo : IDiscoveredInfo
        {
            public string SerializedData;

            public IReadOnlyList<IAssemblyInfo> AssemblyInfos
            {
                get { throw new NotImplementedException(); }
            }

            public IReadOnlyList<IServiceInfo> ServiceInfos
            {
                get { throw new NotImplementedException(); }
            }

            public IReadOnlyList<IPluginInfo> PluginInfos
            {
                get { throw new NotImplementedException(); }
            }
        }


        [Test]
        public void serialization_with_serializable_discovered_info()
        {
            var c = CreateConfiguration();
            c.DiscoveredInfo = new SerializableDiscoveredInfo() { SerializedData = "It is serialized." };
            YodiiConfiguration copy = SerializeAndDeserialize( c );
            CheckConfiguration( copy );
            Assert.That( copy.DiscoveredInfo, Is.Not.Null );
            Assert.That( copy.DiscoveredInfo, Is.InstanceOf<SerializableDiscoveredInfo>() );
            Assert.That( ((SerializableDiscoveredInfo)copy.DiscoveredInfo).SerializedData, Is.EqualTo( "It is serialized." ) );
        }

        static YodiiConfiguration SerializeAndDeserialize( YodiiConfiguration c )
        {
            YodiiConfiguration copy;
            using( var s = new MemoryStream() )
            {
                BinaryFormatter f = new BinaryFormatter();
                f.Serialize( s, c );
                s.Position = 0;
                copy = (YodiiConfiguration)f.Deserialize( s );
            }
            return copy;
        }

        static void CheckConfiguration( YodiiConfiguration copy )
        {
            Assert.That( copy.Layers.Count, Is.EqualTo( 2 ) );
            Assert.That( copy.Layers[0].Name, Is.EqualTo( "Test" ) );
            Assert.That( copy.Layers[0].IsDefault, Is.False );
            Assert.That( copy.Layers[0].Items.Count, Is.EqualTo( 2 ) );
            Assert.That( copy.Layers[0].Items[0].ServiceOrPluginFullName, Is.EqualTo( "First" ) );
            Assert.That( copy.Layers[0].Items[0].Status, Is.EqualTo( ConfigurationStatus.Running ) );
            Assert.That( copy.Layers[0].Items[0].Impact, Is.EqualTo( StartDependencyImpact.FullStart ) );
            Assert.That( copy.Layers[0].Items[0].Description, Is.EqualTo( String.Empty ) );
            Assert.That( copy.Layers[0].Items[1].ServiceOrPluginFullName, Is.EqualTo( "Second" ) );
            Assert.That( copy.Layers[0].Items[1].Status, Is.EqualTo( ConfigurationStatus.Runnable ) );
            Assert.That( copy.Layers[0].Items[1].Impact, Is.EqualTo( StartDependencyImpact.TryStartRecommended ) );
            Assert.That( copy.Layers[0].Items[1].Description, Is.EqualTo( "The second one." ) );
            Assert.That( copy.Layers[1].Name, Is.EqualTo( "" ) );
            Assert.That( copy.Layers[1].IsDefault );
            Assert.That( copy.Layers[1].Items.Count, Is.EqualTo( 1 ) );
            Assert.That( copy.Layers[1].Items[0].ServiceOrPluginFullName, Is.EqualTo( "OtherFirst" ) );
            Assert.That( copy.Layers[1].Items[0].Status, Is.EqualTo( ConfigurationStatus.Disabled ) );
            Assert.That( copy.Layers[1].Items[0].Impact, Is.EqualTo( StartDependencyImpact.Unknown ) );
            Assert.That( copy.Layers[1].Items[0].Description, Is.EqualTo( String.Empty ) );
        }

        static YodiiConfiguration CreateConfiguration()
        {
            var c = new YodiiConfiguration();
            var lTest = new YodiiConfigurationLayer() { Name = "Test" };
            lTest.Items.Add( new YodiiConfigurationItem() { ServiceOrPluginFullName = "First", Status = ConfigurationStatus.Running, Impact = StartDependencyImpact.FullStart } );
            lTest.Items.Add( new YodiiConfigurationItem() { ServiceOrPluginFullName = "Second", Status = ConfigurationStatus.Runnable, Impact = StartDependencyImpact.TryStartRecommended, Description = "The second one." } );
            c.Layers.Add( lTest );
            var lDefault = new YodiiConfigurationLayer();
            lDefault.Items.Add( new YodiiConfigurationItem() { ServiceOrPluginFullName = "OtherFirst", Status = ConfigurationStatus.Disabled } );
            c.Layers.Add( lDefault );
            return c;
        }


    }
}
