#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\ConfigurationManager\ConfigurationFailureResult.cs) is part of CiviKey. 
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
using System.Diagnostics;
using System.Linq;
using System.Text;
using CK.Core;
using Yodii.Model;

namespace Yodii.Engine
{
    class ConfigurationFailureResult : IConfigurationFailureResult
    {
        readonly IReadOnlyList<string> _failureReasons;

        /// <summary>
        /// Initializes a new success result.
        /// </summary>
        internal ConfigurationFailureResult()
        {
        }

        /// <summary>
        /// Initializes a failure result from the FillConfiguration: only one
        /// blocking condition can be detected.
        /// </summary>
        /// <param name="reason"></param>
        internal ConfigurationFailureResult( string reason )
        {
            Debug.Assert( !String.IsNullOrWhiteSpace( reason ) );
            _failureReasons = new CKReadOnlyListMono<string>( reason );
        }

        /// <summary>
        /// Initializes a failure result due to cancellations by external code (from ConfigurationChanging event).
        /// </summary>
        /// <param name="reasons">Multiple reasons.</param>
        internal ConfigurationFailureResult( IReadOnlyList<string> reasons )
        {
            Debug.Assert( reasons != null && reasons.Count > 0 );
            _failureReasons = reasons;
        }

        public bool Success
        {
            get { return _failureReasons == null; }
        }

        public IReadOnlyList<string> FailureReasons
        {
            get { return _failureReasons; }
        }

    }
}
