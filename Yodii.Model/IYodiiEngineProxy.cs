#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Model\IYodiiEngineInternal.cs) is part of CiviKey. 
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

namespace Yodii.Model
{
    /// <summary>
    /// This is the internal view of the engine: plugins can interact with it since it can be injected
    /// into the plugin constructor.
    /// <para>
    /// It offers Start/Stop capabilities (see the base <see cref="IYodiiEngineBase"/> methods) and since this is a per plugin view of the engine, the
    /// caller key, when not specified (let to null) is automatically set to the plugin full name (see <see cref="IYodiiEngineBase.StartItem"/>).
    /// </para>
    /// <para>
    /// The external <see cref="IYodiiEngineExternal"/> view is exposed (there is no reason to hide it from a plugin perspective), but
    /// it should not be used directly except for specific plugins that may need to interact closely with the engine (by stopping it for instance).
    /// </para>
    /// </summary>
    public interface IYodiiEngineProxy : IYodiiEngineBase
    {
        IYodiiEngineExternal ExternalEngine { get; }
    }
}
