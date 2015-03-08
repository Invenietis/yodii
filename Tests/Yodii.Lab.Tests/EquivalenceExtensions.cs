#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Lab.Tests\EquivalenceExtensions.cs) is part of CiviKey. 
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

namespace Yodii.Engine.Tests
{
    class EquivalenceExtensions
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
                AssertServiceEquivalence( a.Service, b.Service );
            }

            Assert.That( a.ServiceReferences.Count == b.ServiceReferences.Count );
            foreach(var referenceB in b.ServiceReferences)
            {
                var referenceA = a.ServiceReferences.Where( x => x.Owner.PluginFullName == referenceB.Owner.PluginFullName &&
                    x.Reference.ServiceFullName == referenceB.Reference.ServiceFullName &&
                    x.Requirement == referenceB.Requirement ).FirstOrDefault();

                Assert.That( referenceA != null );

                AssertServiceEquivalence( referenceA.Reference, referenceB.Reference );
            }
        }

        /// <summary>
        /// Assert equivalence between two IServiceInfo, in the context of Yodii.Lab.
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        public static void AssertServiceEquivalence( IServiceInfo a, IServiceInfo b )
        {
            if( a == null && b == null ) return;

            Assert.That( a != null && b != null );
            Assert.That( a.ServiceFullName == b.ServiceFullName );
            AssertServiceEquivalence( a.Generalization, b.Generalization );
            
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
                    Assert.That( layerB.Items.Any( x => x.ServiceOrPluginFullName == item.ServiceOrPluginFullName && x.Status == item.Status ) );
                }
            }

            if( a.FinalConfiguration != null )
            {
                Assert.That( b.FinalConfiguration != null );
                Assert.That( a.FinalConfiguration.Items.Count == b.FinalConfiguration.Items.Count );
                foreach( var item in a.FinalConfiguration.Items )
                {
                    Assert.That( b.FinalConfiguration.Items.Any( x => x.ServiceOrPluginFullName == item.ServiceOrPluginFullName && x.Status == item.Status ) );
                }
            }
        }

        /// <summary>
        /// Asserts that both DiscoveredInfo are equivalent.
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        public static void AssertDiscoveredInfoEquivalence( IDiscoveredInfo a, IDiscoveredInfo b )
        {
            Assert.That( a.PluginInfos.Count == b.PluginInfos.Count );
            Assert.That( a.ServiceInfos.Count == b.ServiceInfos.Count );

            foreach( var sA in a.ServiceInfos )
            {
                var sB = b.ServiceInfos.First( s => sA.ServiceFullName == s.ServiceFullName );

                AssertServiceEquivalence( sA, sB );
            }

            foreach( var pA in a.PluginInfos )
            {
                var pB = b.PluginInfos.First( p => pA.PluginFullName == p.PluginFullName );

                AssertPluginEquivalence( pA, pB, true );
            }

            Assert.That( a.IsValid() == b.IsValid() );
        }

        /// <summary>
        /// Asserts that both engines have equivalent static infos.
        /// </summary>
        /// <param name="a">A</param>
        /// <param name="b">B</param>
        public static void AssertEngineInfoEquivalence(IYodiiEngineExternal a, IYodiiEngineExternal b)
        {
            AssertDiscoveredInfoEquivalence( a.Configuration.DiscoveredInfo, b.Configuration.DiscoveredInfo );

            AssertManagerEquivalence( a.Configuration, b.Configuration );
        }
    }
}
