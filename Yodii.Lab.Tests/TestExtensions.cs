
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Yodii.Model;

namespace Yodii.Lab.Tests
{
    class TestExtensions
    {
        /// <summary>
        /// Assert equivalence between two IPluginInfo, in the context of Yodii.Lab.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AssertPluginEquivalence( IPluginInfo a, IPluginInfo b, bool inspectServices = false )
        {
            if( a == null && b == null ) return;

            Assert.That( a != null && b != null );

            Assert.That( a.PluginFullName == b.PluginFullName );

            if( a.Service == null )
                Assert.That( b.Service == null );
            else if( inspectServices )
            {
                AssertServiceEquivalence( a.Service, b.Service, false );
            }

            Assert.That( a.ServiceReferences.Count == b.ServiceReferences.Count );
            foreach(var referenceB in b.ServiceReferences)
            {
                var referenceA = a.ServiceReferences.Where( x => x.Owner.PluginFullName == referenceB.Owner.PluginFullName &&
                    x.Reference.ServiceFullName == referenceB.Reference.ServiceFullName &&
                    x.Requirement == referenceB.Requirement ).FirstOrDefault();

                Assert.That( referenceA != null );

                AssertServiceEquivalence( referenceA.Reference, referenceB.Reference, false );
            }
        }

        /// <summary>
        /// Assert equivalence between two IServiceInfo, in the context of Yodii.Lab.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AssertServiceEquivalence( IServiceInfo a, IServiceInfo b, bool inspectPlugins = false )
        {
            if( a == null && b == null ) return;

            Assert.That( a != null && b != null );
            Assert.That( a.ServiceFullName == b.ServiceFullName );
            AssertServiceEquivalence( a.Generalization, b.Generalization, inspectPlugins );

            Assert.That( a.Implementations.Count == b.Implementations.Count );
            if( inspectPlugins )
            {
                foreach( var pluginB in b.Implementations )
                {
                    Assert.That( a.Implementations.Where( x => x.PluginFullName == pluginB.PluginFullName ).Count() == 1 );
                    var pluginA = a.Implementations.Where( x => x.PluginFullName == pluginB.PluginFullName ).First();

                    AssertPluginEquivalence( pluginA, pluginB, false );
                }
            }
            
        }

        /// <summary>
        /// Assert equivalence between two IServiceReferenceInfo, in the context of Yodii.Lab.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AssertServiceReferenceEquivalence( IServiceReferenceInfo a, IServiceReferenceInfo b )
        {
            if( a == null && b == null ) return;

            Assert.That( a != null && b != null );
        }

        /// <summary>
        /// Asserts equivalence between two IConfigurationManager.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AssertManagerEquivalence( IConfigurationManager a, IConfigurationManager b )
        {
            if( a == null && b == null ) return;

            Assert.That( a != null && b != null );

            Assert.That( a.Layers.Count == b.Layers.Count );

            for( int i = 0; i < a.Layers.Count; i++ )
            {
                // Consider equivalent if they're in the exact same order?
                var layerA = a.Layers[i];
                var layerB = b.Layers[i];

                Assert.That( layerA.LayerName == layerB.LayerName );

                foreach( var item in layerA.Items )
                {
                    Assert.That( layerB.Items.Any( x => x.ServiceOrPluginId == item.ServiceOrPluginId && x.Status == item.Status ) );
                }
            }

            if( a.FinalConfiguration != null )
            {
                Assert.That( b.FinalConfiguration != null );
                Assert.That( a.FinalConfiguration.Items.Count == b.FinalConfiguration.Items.Count );
                foreach( var item in a.FinalConfiguration.Items )
                {
                    Assert.That( b.FinalConfiguration.Items.Any( x => x.ServiceOrPluginId == item.ServiceOrPluginId && x.Status == item.Status ) );
                }
            }
        }
    }
}
