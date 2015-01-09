#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Discoverer.Tests\DiscovererTests.cs) is part of CiviKey. 
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

using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Yodii.Model;
using Yodii.Discoverer;
//using Mono.Cecil;
using CK.Core;

namespace Yodii.Discoverer.Tests
{
    [TestFixture]
    class DiscovererTests
    {
        [Test]
        public void ChoucrouteTest1()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            IAssemblyInfo ia = discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.ChoucroutePlugin" ).Service == info.ServiceInfos.First( s => s.ServiceFullName == "Yodii.Discoverer.Tests.IChoucrouteService" ) );
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.ChoucroutePlugin" ).ServiceReferences.FirstOrDefault().Reference == info.ServiceInfos.First( s => s.ServiceFullName == "Yodii.Discoverer.Tests.IAnotherService" ) );
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.ChoucroutePlugin" ).ServiceReferences.FirstOrDefault().Requirement == DependencyRequirement.Optional );
        }

        [Test]
        public void ChoucrouteTest2()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.First( p => p.PluginFullName == "Yodii.Discoverer.Tests.Plugin1" ).Service == info.ServiceInfos.First( s => s.ServiceFullName == "Yodii.Discoverer.Tests.IService2" ) );
            Assert.That( !info.PluginInfos.Any( p => p.PluginFullName == "Yodii.Discoverer.Tests.UntaggedPlugin" ) );
        }

        [Test]
        public void ChoucrouteTest3()
        {
            StandardDiscoverer discoverer = new StandardDiscoverer();
            discoverer.ReadAssembly( Path.GetFullPath( "Yodii.Discoverer.Tests.dll" ) );
            IDiscoveredInfo info = discoverer.GetDiscoveredInfo();
            Assert.That( info.PluginInfos.Any( p => p.PluginFullName == "Yodii.Discoverer.Tests.DerivedClass" ) );
        }
    }
}
