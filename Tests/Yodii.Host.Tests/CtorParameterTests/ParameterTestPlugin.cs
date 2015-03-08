#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\CtorParameterTests\ParameterTestPlugin.cs) is part of CiviKey. 
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
    public class MyCustomClass
    {
        public string TestParameter { get; set; }
    }

    public class ParameterTestPlugin : YodiiPluginBase
    {
        readonly ITrackMethodCallsPluginService _tracker;
        readonly MyCustomClass _testClass;

        public ParameterTestPlugin( MyCustomClass testClass, ITrackMethodCallsPluginService tracker, IRunnableService<IFailureTransitionPluginService> otherService )
        {
            _tracker = tracker;
            _testClass = testClass;
            _testClass.TestParameter = "Ctor";
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.DoSomething() );
        }

        protected override void PluginPreStart( IPreStartContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.DoSomething() );
            _testClass.TestParameter = "PreStart";
        }

        protected override void PluginPreStop( IPreStopContext c )
        {
            _tracker.DoSomething();
            _testClass.TestParameter = "PreStop";
        }

        protected override void PluginStart( IStartContext c )
        {
            _tracker.DoSomething();
            _testClass.TestParameter = "Start";
        }

        protected override void PluginStop( IStopContext c )
        {
            Assert.Throws<ServiceCallBlockedException>( () => _tracker.DoSomething() );
            _testClass.TestParameter = "Stop";
        }
    }
}
