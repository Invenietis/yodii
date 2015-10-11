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
    [Serializable]
    internal sealed class PluginInfo : IPluginInfo, IDiscoveredItem, IPluginCtorInfo
    {
        readonly string _pluginFullName;
        readonly IAssemblyInfo _assemblyInfo;
        readonly IReadOnlyList<IServiceReferenceInfo> _serviceReferences;
        readonly int _parameterCount;
        readonly IReadOnlyList<IPluginCtorKnownParameterInfo> _knownParameters;
        readonly IServiceInfo _service;
        readonly string _errorMessage;

        internal PluginInfo( string pluginFullName, IAssemblyInfo assemblyInfo, IServiceInfo implService, List<ServiceReferenceInfo> services, int ctorParameterCount, List<PluginInfoKnownParameter> knownParameters )
        {
            Debug.Assert( !String.IsNullOrEmpty( pluginFullName ) );
            Debug.Assert( assemblyInfo != null );

            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;
            _service = implService;
            _parameterCount = ctorParameterCount;
            if( services != null )
            {
                foreach( var sRef in services ) sRef.Owner = this;
                _serviceReferences = services.ToReadOnlyList();
            }
            else _serviceReferences = CKReadOnlyListEmpty<ServiceReferenceInfo>.Empty;
            _knownParameters = knownParameters != null ? knownParameters.ToReadOnlyList() : CKReadOnlyListEmpty<PluginInfoKnownParameter>.Empty;
        }

        internal PluginInfo( string pluginFullName, IAssemblyInfo assemblyInfo, string errorMessage )
        {
            _pluginFullName = pluginFullName;
            _assemblyInfo = assemblyInfo;
            _errorMessage = errorMessage;
            _serviceReferences = CKReadOnlyListEmpty<ServiceReferenceInfo>.Empty;
            _knownParameters = CKReadOnlyListEmpty<PluginInfoKnownParameter>.Empty;
        }

        public string PluginFullName
        {
            get { return _pluginFullName; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public IReadOnlyList<IServiceReferenceInfo> ServiceReferences
        {
            get { return _serviceReferences; }
        }

        public IServiceInfo Service
        {
            get { return _service; }
        }

        public IPluginCtorInfo ConstructorInfo
        {
            get { return this; }
        }

        int IPluginCtorInfo.ParameterCount
        {
            get { return _parameterCount; }
        }

        IReadOnlyList<IPluginCtorKnownParameterInfo> IPluginCtorInfo.KnownParameters
        {
            get { return _knownParameters; }
        }

        public bool HasError
        {
            get { return _errorMessage != null; }
        }

        public string ErrorMessage
        {
            get { return _errorMessage; }
        }

    }
}
