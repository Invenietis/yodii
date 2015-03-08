#region LGPL License
/*----------------------------------------------------------------------------
* This file (Yodii.Host\Plugin\StContext.cs) is part of CiviKey. 
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

namespace Yodii.Host
{
    class StContext
    {
        readonly Dictionary<object, object> _shared;
        CancellationInfo _info;

        public readonly PluginProxy Plugin;

        public StContext( PluginProxy plugin, Dictionary<object, object> shared )
        {
            Plugin = plugin;
            _shared = shared;
        }

        public ServiceManager.Impact ServiceImpact { get; set; }

        public virtual void Cancel( string message = null, Exception ex = null )
        {
            _info = new CancellationInfo( Plugin.PluginInfo ) { ErrorMessage = message, Error = ex };
        }

        public bool Success
        {
            get { return _info == null; }
        }

        public bool HandleSuccess( List<CancellationInfo> errors, bool isPreStart )
        {
            if( _info == null ) return true;
            _info.IsStartCanceled = isPreStart;
            errors.Add( _info );
            return false;
        }

        public IDictionary<object, object> SharedMemory { get { return _shared; } }

        public enum StStatus
        {
            Stopping = 1,
            Starting = 2,
            IsSwapping = 8,
            StoppingSwap = Stopping | IsSwapping,
            StartingSwap = Starting | IsSwapping,
            IsHotSwapping = IsSwapping + 16,
            StoppingHotSwap = Stopping | IsHotSwapping,
            StartingHotSwap = Starting | IsHotSwapping,
        }

        public StStatus Status { get; set; }

        /// <summary>
        /// Used when this is a PreStopContext that is a PreStartContext.PreviousPlugin.
        /// </summary>
        internal bool HotSwapped 
        {
            get { return Status > StStatus.IsHotSwapping; }
            set 
            { 
                if( value ) Status |= StStatus.IsHotSwapping; 
                else Status &= ~StStatus.IsHotSwapping; 
            } 
        }
    }

}
