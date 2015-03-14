#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\StStartContext.cs) is part of CiviKey. 
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

namespace Yodii.Host
{
    [DebuggerDisplay( "Implementation={Implementation}" )]
    class StStartContext : StContext, IPreStartContext, IStartContext
    {
        ServiceManager.Impact _swappedImpact;

        public StStartContext( PluginProxy plugin, RunningStatus status, Dictionary<object, object> shared, bool wasDisabled )
            : base( plugin, status, shared )
        {
            WasDisabled = wasDisabled;
        }

        public readonly bool WasDisabled;

        public Action<IStopContext> RollbackAction { get; set; }

        public ServiceManager.Impact SwappedServiceImpact 
        {
            get { return _swappedImpact; } 
            set
            {
                Debug.Assert( _swappedImpact == null && value != null );
                _swappedImpact = value;
                Status = StContext.StStatus.StartingSwap;
            }
        }

        public IYodiiService PreviousPluginCommonService 
        {
            get { return _swappedImpact != null ? (IYodiiService)_swappedImpact.Service : null; } 
        }

        public StContext PreviousImpl 
        {
            get { return _swappedImpact != null ? _swappedImpact.Implementation : null; } 
        }

        IYodiiPlugin IPreStartContext.PreviousPlugin { get { return PreviousImpl != null ? PreviousImpl.Plugin.RealPluginObject : null; } }

        bool IPreStartContext.HotSwapping
        {
            get { return Status == StStatus.StartingHotSwap; }
            set
            {
                if( PreviousImpl == null ) throw new InvalidOperationException( R.PreviousPluginMustNotBeNull );
                PreviousImpl.HotSwapped = value;
                HotSwapped = value;
            }
        }

        bool IStartContext.CancellingPreStop
        {
            get { return false; }
        }

        bool IStartContext.HotSwapping
        {
            get { return Status == StStatus.StartingHotSwap; }
        }

        public override string ToString()
        {
            return String.Format( "Start: {0}, Previous={1}", Plugin.PluginInfo.PluginFullName, PreviousImpl != null ? PreviousImpl.Plugin.PluginInfo.PluginFullName : "null" );
        }
    }

}
