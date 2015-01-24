#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Engine\EmptyDiscoveredInfo.cs) is part of CiviKey. 
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
using Yodii.Model;
using CK.Core;
using System.Runtime.Serialization;

namespace Yodii.Engine
{
    /// <summary>
    /// Empty object pattern implementation of <see cref="IDiscoveredInfo"/>
    /// as a (serializable) singleton.
    /// </summary>
    [Serializable]
    public sealed class EmptyDiscoveredInfo : IDiscoveredInfo, ISerializable
    {
        /// <summary>
        /// Unique singleton object.
        /// </summary>
        public static readonly IDiscoveredInfo Empty = new EmptyDiscoveredInfo();

        EmptyDiscoveredInfo()
        {
        }

        IReadOnlyList<IServiceInfo> IDiscoveredInfo.ServiceInfos
        {
            get { return CKReadOnlyListEmpty<IServiceInfo>.Empty; }
        }

        IReadOnlyList<IPluginInfo> IDiscoveredInfo.PluginInfos
        {
            get { return CKReadOnlyListEmpty<IPluginInfo>.Empty; }
        }

        IReadOnlyList<IAssemblyInfo> IDiscoveredInfo.AssemblyInfos
        {
            get { return CKReadOnlyListEmpty<IAssemblyInfo>.Empty; }
        }

        void ISerializable.GetObjectData( SerializationInfo info, StreamingContext context )
        {
            info.SetType( typeof( SingletonSerializationHelper ) );
        }

        [Serializable]
        sealed class SingletonSerializationHelper : IObjectReference
        {
            public object GetRealObject( StreamingContext context )
            {
                return Empty;
            }
        }
    }
}
