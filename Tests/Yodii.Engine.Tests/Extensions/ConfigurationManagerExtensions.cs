#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Engine.Tests\Extensions\ConfigurationManagerExtensions.cs) is part of CiviKey. 
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

namespace Yodii
{
    static class ConfigurationManagerExtensions
    {
        public static IYodiiEngineResult AddSuccess( this IConfigurationItemCollection @this, string name, ConfigurationStatus status, ConfigurationStatus? solvedStatusInFinalConfig = null )
        {
            IYodiiEngineResult r = @this.Set( name, status );
            r.CheckSuccess();
            if( solvedStatusInFinalConfig != null ) Assert.That( @this.ParentLayer.ConfigurationManager.FinalConfiguration.GetStatus( name ), Is.EqualTo( solvedStatusInFinalConfig ) );
            return r;
        }

        public static void CheckFinalConfigurationItemStatus( this IConfigurationManager @this, params string[] segment )
        {
            for( int i = 0; i < segment.Length ; i++) segment[i] += ",";
            CheckFinalConfigurationItemStatus( @this, String.Concat( segment ) );
        }

        public static void CheckFinalConfigurationItemStatus( this IConfigurationManager @this, string segment )
        {
            var items = segment.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
            foreach( var i in items )
            {
                var split = i.Split( new[] { '=' }, StringSplitOptions.RemoveEmptyEntries );
                if( split.Length > 2 ) Assert.Fail( "expected [serviceOrPluginName]=[status]" );
                Assert.That( @this.FinalConfiguration.GetStatus( split[0] ).ToString(), Is.EqualTo( split[1] ) );
            }
        }

        public static void CheckFinalConfigurationItemImpact( this IConfigurationManager @this, params string[] segment )
        {
            for( int i = 0; i < segment.Length; i++ ) segment[i] += ",";
            CheckFinalConfigurationItemImpact( @this, String.Concat( segment ) );
        }

        public static void CheckFinalConfigurationItemImpact( this IConfigurationManager @this, string segment )
        {
            var items = segment.Split( new[] { ',' }, StringSplitOptions.RemoveEmptyEntries ).Select( s => s.Trim() );
            foreach( var i in items )
            {
                var split = i.Split( new[] { '=' }, StringSplitOptions.RemoveEmptyEntries );
                if( split.Length > 2 ) Assert.Fail( "expected [serviceOrPluginName]=[impact]" );
                Assert.That( @this.FinalConfiguration.GetImpact( split[0] ).ToString(), Is.EqualTo( split[1] ) );
            }
        }
    }
}
