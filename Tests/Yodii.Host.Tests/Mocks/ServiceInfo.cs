#region LGPL License
/*----------------------------------------------------------------------------
* This file (Tests\Yodii.Host.Tests\Mocks\ServiceInfo.cs) is part of CiviKey. 
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
using CK.Core;
using Yodii.Model;

namespace Yodii.Host.Tests.Mocks
{
    public class ServiceInfo : IServiceInfo
    {
        readonly string _serviceFullName;
        readonly IAssemblyInfo _assemblyInfo;
        readonly List<IPluginInfo> _implementations;
        IServiceInfo _generalization;
        bool _hasError; 

        internal ServiceInfo( string serviceFullName, IAssemblyInfo assemblyInfo )
        {
            Debug.Assert( !String.IsNullOrEmpty( serviceFullName ) );
            Debug.Assert( assemblyInfo != null );

            _serviceFullName = serviceFullName;
            _assemblyInfo = assemblyInfo;
            _implementations = new List<IPluginInfo>();
        }

        internal void AddPlugin( IPluginInfo plugin )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( plugin.Service == this );
            Debug.Assert( !_implementations.Contains( plugin ) );
            _implementations.Add( plugin );
        }

        internal void RemovePlugin( IPluginInfo plugin )
        {
            Debug.Assert( plugin != null );
            Debug.Assert( plugin.Service == this );
            Debug.Assert( _implementations.Contains( plugin ) );
            _implementations.Remove( plugin );
        }

        #region IServiceInfo Members

        public string ServiceFullName
        {
            get { return _serviceFullName; }
        }

        public IServiceInfo Generalization
        {
            get { return _generalization; }
            set { _generalization = value; }
        }

        public IAssemblyInfo AssemblyInfo
        {
            get { return _assemblyInfo; }
        }

        public IReadOnlyList<IPluginInfo> Implementations
        {
            get { return _implementations.AsReadOnlyList(); }
        }

        #endregion

        public bool HasError
        {
            get { return _hasError; }
            set { _hasError = value; }
        }

        public string ErrorMessage
        {
            get { return _hasError ? "An error occured." : null; }
        }

    }
}
