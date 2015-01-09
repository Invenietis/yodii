#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Lab\Mock model\MockServiceReferenceInfo.cs) is part of CiviKey. 
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
using System.Threading.Tasks;
using Yodii.Model;


namespace Yodii.Lab.Mocks
{
    /// <summary>
    /// Mock IServiceReferenceInfo.
    /// </summary>
    [DebuggerDisplay( "=> {Reference.ServiceFullName} ({Requirement})" )]
    public class MockServiceReferenceInfo : ViewModelBase, IServiceReferenceInfo
    {
        readonly PluginInfo _owner;
        readonly ServiceInfo _reference;

        DependencyRequirement _requirement;

        internal MockServiceReferenceInfo( PluginInfo ownerPlugin, ServiceInfo referencedService, DependencyRequirement requirement )
        {
            Debug.Assert( ownerPlugin != null );
            Debug.Assert( referencedService != null );

            _owner = ownerPlugin;
            _reference = referencedService;
            _requirement = requirement;
        }
        #region IServiceReferenceInfo Members

        IPluginInfo IServiceReferenceInfo.Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Plugin owner.
        /// </summary>
        public PluginInfo Owner
        {
            get { return _owner; }
        }

        /// <summary>
        /// Service ref.
        /// </summary>
        public IServiceInfo Reference
        {
            get { return _reference; }
        }

        DependencyRequirement IServiceReferenceInfo.Requirement
        {
            get { return _requirement; }
        }

        /// <summary>
        /// Requirement.
        /// </summary>
        public DependencyRequirement Requirement
        {
            get { return _requirement; }
            set
            {
                if( value != _requirement)
                {
                    _requirement = value;
                    RaisePropertyChanged( "Requirement" );
                }
            }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public string ConstructorParameterName
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public int ConstructorParameterIndex
        {
            get { throw new NotImplementedException(); }
        }

        /// <summary>
        /// Not implemented.
        /// </summary>
        public bool IsNakedRunningService
        {
            get { throw new NotImplementedException(); }
        }

        #endregion


        /// <summary>
        /// Not implemented.
        /// </summary>
        public string ConstructorParmeterOrPropertyName
        {
            get { throw new NotImplementedException(); }
        }
    }
}
