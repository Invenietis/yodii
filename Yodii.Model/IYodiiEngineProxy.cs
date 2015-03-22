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
        /// <summary>
        /// Gets the <see cref="IYodiiEngineExternal"/> engine façade.
        /// </summary>
        IYodiiEngineExternal ExternalEngine { get; }

        /// <summary>
        /// Fires whenever <see cref="IsRunningLocked"/> changes.
        /// </summary>
        event EventHandler IsRunningLockedChanged;

        /// <summary>
        /// Gets whether the plugin is <see cref="RunningStatus.RunningLocked"/> or only <see cref="RunningStatus.Running"/>.
        /// This property is updated once the PreStart/Stop phase succeeded, but before Start and Stop methods are called.
        /// </summary>
        bool IsRunningLocked { get; }

        /// <summary>
        /// Gets whether the plugin has locked itself in running mode. See <see cref="SelfLock"/> and <see cref="SelfUnlock"/>.
        /// </summary>
        bool IsSelfLocked { get; }

        /// <summary>
        /// Locks the plugin in <see cref="RunningStatus.RunningLocked"/> mode by adding a <see cref="ConfigurationStatus.Running"/> configuration
        /// to the "Self-Locking" configuration layer of the engine.
        /// This can be call at any moment by the running plugin but MUST NOT be called from its <see cref="IYodiiPlugin.Start"/>, 
        /// <see cref="IYodiiPlugin.PreStart"/>, <see cref="IYodiiPlugin.PreStop"/> or <see cref="IYodiiPlugin.Stop"/> methods
        /// otherwise an <see cref="InvalidOperationException"/> is thrown. See remarks.
        /// </summary>
        /// <remarks>
        /// <para>
        /// This method fails if and only if the <see cref="IConfigurationManager.ConfigurationChanging"/> event has been canceled.
        /// </para>
        /// <para>
        /// Currently it can not be called from the <see cref="IYodiiPlugin.Start"/> method of the plugin. However, it would be great... but 
        /// this is a very special case of reentrancy (from host to configuration back to configuration and to the host) that has yet to be handled.
        /// If a plugin needs to lock itself directly from its Start method, it can use the <see cref="IStartContext.PostAction"/> to call this SelfLock method.
        /// </para>
        /// </remarks>
        /// <returns>
        /// True on success, false on failure: since the plugin is actually running, this can only be 
        /// rejected by the <see cref="IConfigurationManager.ConfigurationChanging"/> event.
        /// </returns>
        bool SelfLock();

        /// <summary>
        /// Unlocks the plugin (see <see cref="SelfLock"/>) by removing the configuration.
        /// This can be call at any moment by the running plugin and also from its <see cref="IYodiiPlugin.Start"/> method.
        /// This MUST NOT be called from <see cref="IYodiiPlugin.PreStart"/>, <see cref="IYodiiPlugin.PreStop"/> or <see cref="IYodiiPlugin.Stop"/> methods
        /// otherwise an <see cref="InvalidOperationException"/> is thrown.
        /// </summary>
        void SelfUnlock();

    }
}
