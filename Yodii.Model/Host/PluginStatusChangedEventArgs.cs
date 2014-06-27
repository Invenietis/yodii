#region LGPL License
/*----------------------------------------------------------------------------
* This file (CK.Plugin.Model\Host\PluginStatusChangedEventArgs.cs) is part of CiviKey. 
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
* Copyright © 2007-2012, 
*     Invenietis <http://www.invenietis.com>,
*     In’Tech INFO <http://www.intechinfo.fr>,
* All rights reserved. 
*-----------------------------------------------------------------------------*/
#endregion

using System;
using CK.Core;

namespace Yodii.Model
{
	/// <summary>
	/// Event argument when a plugin <see cref="InternalRunningStatus">status</see> changed.
	/// </summary>
	public class PluginStatusChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the plugin proxy.
        /// </summary>
        public IPluginProxy PluginProxy { get; private set; }

        /// <summary>
		/// Gets the previous status.
		/// </summary>
		public InternalRunningStatus Previous { get; private set; }
		
        /// <summary>
        /// Initializes a new instance of a <see cref="PluginStatusChangedEventArgs"/>.
        /// </summary>
        /// <param name="previous">The previous running status.</param>
        /// <param name="current">The plugin proxy.</param>
        public PluginStatusChangedEventArgs( InternalRunningStatus previous, IPluginProxy pluginProxy )
		{
			Previous = previous;
			PluginProxy = pluginProxy;
		}

	}

}
