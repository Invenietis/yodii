#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Discoverer\Discoverer\PluginInfo.cs) is part of CiviKey. 
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
using Yodii.Model;
using CK.Core;
using System.Runtime.InteropServices;

namespace Yodii.Discoverer
{
    internal sealed class PluginInfo : IPluginInfo, IDiscoveredItem
    {
        readonly string _pluginFullName;
        readonly IAssemblyInfo _assemblyInfo;
        List<IServiceReferenceInfo> _serviceReferences;
        IServiceInfo _service;
        string _errorMessage;

        internal PluginInfo( string pluginFullName, IAssemblyInfo assemblyInfo )
        {
            Debug.Assert( !String.IsNullOrEmpty( pluginFullName ) );
            Debug.Assert( assemblyInfo != null );

            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;
            _serviceReferences = new List<IServiceReferenceInfo>();
        }

        internal void BindServiceRequirement( IServiceReferenceInfo reference )
        {
            Debug.Assert( !_serviceReferences.Contains( reference ) );
            Debug.Assert( reference.Owner == this );

            _serviceReferences.Add( reference );
        }

        #region IPluginInfo Members

        public string PluginFullName
        {
            get { return _pluginFullName; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public List<IServiceReferenceInfo> ServiceReferencess
        {
            get { return _serviceReferences; }
        }

        public IServiceInfo Service
        {
            get { return _service; }
            internal set { _service = value; }
        }

        #endregion

        public bool HasError
        {
            get { return _errorMessage != null; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set { _errorMessage = value; }
        }

        IReadOnlyList<IServiceReferenceInfo> IPluginInfo.ServiceReferences
        {
            get { return _serviceReferences.AsReadOnlyList(); }
        }
    }
}
