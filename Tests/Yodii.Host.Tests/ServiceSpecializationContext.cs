#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\ServiceSpecializationContext.cs) is part of CiviKey. 
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
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using NUnit.Framework;
using Yodii.Discoverer;
using Yodii.Engine;
using Yodii.Model;

namespace Yodii.Host.Tests
{
    class ServiceSpecializationContext
    {
        public readonly ServiceWrapper<ITestRootService> ServiceRoot;
        public readonly ServiceWrapper<ITestRootSubAService> ServiceSubA;
        public readonly ServiceWrapper<ITestRootSubBService> ServiceSubB;
        public readonly ServiceWrapper<ITestRootSubBSubService> ServiceSubBSub;

        public readonly PluginWrapper<TestRootRootPlugin> PluginRoot;
        public readonly PluginWrapper<TestRootPluginAlternate> AltPluginRoot;
        public readonly PluginWrapper<TestRootSubAPlugin> PluginSubA;
        public readonly PluginWrapper<TestRootSubAPluginAlternate> AltPluginSubA;
        public readonly PluginWrapper<TestRootSubBPlugin> PluginSubB;
        public readonly PluginWrapper<TestRootSubBPluginAlternate> AltPluginSubB;
        public readonly PluginWrapper<TestRootSubBSubPlugin> PluginSubBSub;
        public readonly PluginWrapper<TestRootSubBSubPluginAlernate> AltPluginSubBSub;

        public readonly YodiiEngine Engine;

        public readonly YodiiHost Host;

        ServiceWrapper[] _allServices;
        PluginWrapper[] _allPlugins;

        public ServiceSpecializationContext( bool startEngine = true )
        {
            Host = new YodiiHost();
            Engine = new YodiiEngine( Host );
            Engine.Configuration.SetDiscoveredInfo( TestHelper.GetDiscoveredInfoInThisAssembly() );

            ServiceRoot = new ServiceWrapper<ITestRootService>( Engine, Host );
            ServiceSubA = new ServiceWrapper<ITestRootSubAService>( Engine, Host );
            ServiceSubB = new ServiceWrapper<ITestRootSubBService>( Engine, Host );
            ServiceSubBSub = new ServiceWrapper<ITestRootSubBSubService>( Engine, Host );
            _allServices = new ServiceWrapper[] { ServiceRoot, ServiceSubA, ServiceSubB, ServiceSubBSub, ServiceSubBSub };

            PluginRoot = new PluginWrapper<TestRootRootPlugin>( Engine, Host, ServiceRoot );
            AltPluginRoot = new PluginWrapper<TestRootPluginAlternate>( Engine, Host, ServiceRoot );
            PluginSubA = new PluginWrapper<TestRootSubAPlugin>( Engine, Host, ServiceSubA );
            AltPluginSubA = new PluginWrapper<TestRootSubAPluginAlternate>( Engine, Host, ServiceSubA );
            PluginSubB = new PluginWrapper<TestRootSubBPlugin>( Engine, Host, ServiceSubB );
            AltPluginSubB = new PluginWrapper<TestRootSubBPluginAlternate>( Engine, Host, ServiceSubB );
            PluginSubBSub = new PluginWrapper<TestRootSubBSubPlugin>( Engine, Host, ServiceSubBSub );
            AltPluginSubBSub = new PluginWrapper<TestRootSubBSubPluginAlernate>( Engine, Host, ServiceSubBSub );
            _allPlugins = new PluginWrapper[] { PluginRoot, AltPluginRoot, PluginSubA, AltPluginSubA, PluginSubB, AltPluginSubB, PluginSubBSub, AltPluginSubBSub };

            if( startEngine )
            {
                Engine.StartEngine().CheckSuccess();
                _allServices.CheckState( ServiceStatus.Stopped );
                _allPlugins.CheckState( PluginStatus.Null );
                Assert.That( _allServices.AllEvents(), Is.Empty );
            }
        }

        public IReadOnlyList<ServiceWrapper> AllServices { get { return _allServices; } }

        public IReadOnlyList<PluginWrapper> AllPlugins { get { return _allPlugins; } }

        public void CheckOnePluginStarted( PluginWrapper p )
        {
            p.CheckState( PluginStatus.Started );
            Assert.That( AllPlugins.Where( w => w != p ).All( w => w.CheckStoppedOrNull() != null ) );
        }

    }

}
