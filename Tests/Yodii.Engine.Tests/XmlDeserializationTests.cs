#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\XmlDeserializationTests.cs) is part of CiviKey. 
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
using Yodii.Engine.Tests.ConfigurationSolverTests;
using Yodii.Engine.Tests.Mocks;

namespace Yodii.Engine.Tests
{
    [TestFixture]
    class XmlDeserializationTests
    {
        [Test]
        public void TestXmlDeserialization()
        {
            YodiiEngine engineA = MockXmlUtils.CreateEngineFromXmlResource( "Valid001a" );
            YodiiEngine engineB = StaticConfigurationTests.CreateValid001a();

            EquivalenceExtensions.AssertEngineInfoEquivalence( engineA, engineB );

            engineA = MockXmlUtils.CreateEngineFromXmlResource( "Graph005" );
            var info = MockInfoFactory.CreateGraph005();

            EquivalenceExtensions.AssertDiscoveredInfoEquivalence( engineA.Configuration.DiscoveredInfo, info );
        }
    }
}
